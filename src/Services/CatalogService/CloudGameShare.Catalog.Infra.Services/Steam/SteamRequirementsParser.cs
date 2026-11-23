using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;

namespace CloudGameShare.Catalog.Infra.Services.Steam;

internal static class SteamRequirementsParser
{
    public static SteamRequirementsDto? Parse(JsonElement requirements)
    {
        // Steam also sends an empty array for platforms without requirements.
        if (requirements.ValueKind != JsonValueKind.Object)
            return null;

        var minimum = ReadLevel(requirements, "minimum");
        var recommended = ReadLevel(requirements, "recommended");
        return minimum is null && recommended is null ? null : new(minimum, recommended);
    }

    private static SteamRequirementLevelDto? ReadLevel(JsonElement requirements, string name)
    {
        if (!requirements.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
            return null;

        var html = value.GetString() ?? "";
        var text = Replace(html, @"<br\s*/?>|</(?:li|p|ul|div)>", "\n");
        text = WebUtility.HtmlDecode(Replace(text, "<[^>]*>", ""));
        var lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !Match(line, @"^(?:Minimum|Recommended|Mínimos?|Recomendados?)\s*:?$").Success);
        text = string.Join("\n", lines);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var processor = Field(text, "Processor|Processador|CPU");
        var graphics = Field(text, "Graphics|Placa de vídeo|Placa gráfica|Gráficos|Vídeo");
        var memory = Field(text, "Memory|Memória|Memoria|RAM");
        var storage = Field(text, "Storage|Armazenamento|Espaço no disco|Disco rígido");

        return new(text, processor, graphics, CpuCores(processor), Gigabytes(memory), Gigabytes(storage));
    }

    private static string? Field(string text, string labels)
    {
        var match = Match(text, $@"^(?:{labels})\s*:\s*([^\r\n]+)");
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static int? CpuCores(string? processor)
    {
        if (processor is null)
            return null;

        var explicitCount = Match(processor, @"\b(\d{1,3})\s*[- ]?\s*(?:cores?|núcleos?)\b");
        if (explicitCount.Success && int.TryParse(explicitCount.Groups[1].Value, out var count) && count > 0)
            return count;

        var namedCount = Match(processor, @"\b(dual|quad|hexa|octa)[ -]?core\b");
        return namedCount.Groups[1].Value.ToLowerInvariant() switch
        {
            "dual" => 2,
            "quad" => 4,
            "hexa" => 6,
            "octa" => 8,
            _ => null
        };
    }

    private static int? Gigabytes(string? value)
    {
        if (value is null)
            return null;

        var amount = Match(value, @"(?<![\d.,])(\d+(?:[.,]\d+)*)\s*([KMGT]i?B)\b");
        var numericText = amount.Groups[1].Value;
        numericText = Match(numericText, @"^\d{1,3}(?:[.,]\d{3})+$").Success
            ? numericText.Replace(",", "").Replace(".", "")
            : numericText.Replace(',', '.');
        if (!amount.Success || !decimal.TryParse(numericText,
                NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
            return null;

        var gigabytes = amount.Groups[2].Value.ToUpperInvariant() switch
        {
            "KB" or "KIB" => number / 1024 / 1024,
            "MB" or "MIB" => number / 1024,
            "TB" or "TIB" => number * 1024,
            _ => number
        };
        // Catalog allocations are whole GB, so fractions must round up.
        return gigabytes > 0 && gigabytes <= int.MaxValue ? (int)Math.Ceiling(gigabytes) : null;
    }

    private static Match Match(string value, string pattern)
        => Regex.Match(value, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

    private static string Replace(string value, string pattern, string replacement)
        => Regex.Replace(value, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));
}
