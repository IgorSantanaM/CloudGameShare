using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using CloudGameShare.Catalog.Application.Exceptions;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using CloudGameShare.Catalog.Application.Services;
using Microsoft.Extensions.Caching.Memory;

namespace CloudGameShare.Catalog.Infra.Services.Steam;

public sealed class SteamStoreClient(HttpClient httpClient, IMemoryCache cache) : ISteamStoreClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<IReadOnlyList<SteamGameSummaryDto>> SearchAsync(string name, string language, CancellationToken cancellationToken)
    {
        var key = $"steam:search:{language}:{name.ToLowerInvariant()}";
        if (cache.TryGetValue<SteamGameSummaryDto[]>(key, out var cached))
            return cached!;

        var response = await GetAsync<SearchResponse>(
            $"api/storesearch/?term={Uri.EscapeDataString(name)}&l={language}&cc=br", cancellationToken);
        var games = (response.Items ?? [])
            .Where(item => item.Id > 0 && !string.IsNullOrWhiteSpace(item.Name))
            .Take(10)
            .Select(item => new SteamGameSummaryDto(item.Id, item.Name!, item.TinyImage))
            .ToArray();

        cache.Set(key, games, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2), Size = 1 });
        return games;
    }

    public async Task<SteamGameDetailsDto?> GetDetailsAsync(uint appId, string language, CancellationToken cancellationToken)
    {
        var key = $"steam:details:{language}:{appId}";
        if (cache.TryGetValue<SteamGameDetailsDto>(key, out var cached))
            return cached;

        var response = await GetAsync<Dictionary<string, AppResponse>>(
            $"api/appdetails?appids={appId}&l={language}&cc=br", cancellationToken);
        if (!response.TryGetValue(appId.ToString(CultureInfo.InvariantCulture), out var entry)
            || !entry.Success || entry.Data is not { Type: "game" } data
            || data.SteamAppid != appId || string.IsNullOrWhiteSpace(data.Name))
            return null;

        var game = new SteamGameDetailsDto(
            appId, data.Name, PlainText(data.ShortDescription), ParseDate(data.ReleaseDate, language),
            (data.Genres ?? []).Select(genre => genre.Description).Where(genre => !string.IsNullOrWhiteSpace(genre)).ToArray(),
            data.Publishers ?? [], data.Developers ?? [], data.HeaderImage, data.Platforms?.Linux ?? false)
        {
            PcRequirements = SteamRequirementsParser.Parse(data.PcRequirements),
            LinuxRequirements = SteamRequirementsParser.Parse(data.LinuxRequirements)
        };

        cache.Set(key, game, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15), Size = 1 });
        return game;
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T>(path, JsonOptions, cancellationToken)
                ?? throw new JsonException("Steam returned an empty response.");
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException
            || exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new SteamStoreUnavailableException(exception);
        }
    }

    private static string? PlainText(string? value)
        => value is null ? null : WebUtility.HtmlDecode(
            Regex.Replace(value, "<[^>]*>", "", RegexOptions.None, TimeSpan.FromSeconds(1)));

    private static DateOnly? ParseDate(ReleaseDate? release, string language)
    {
        if (release is null || release.ComingSoon)
            return null;

        var culture = CultureInfo.GetCultureInfo(language == "english" ? "en-US" : "pt-BR");
        return DateOnly.TryParse(release.Date, culture, DateTimeStyles.AllowWhiteSpaces, out var date) ? date : null;
    }

    private sealed record SearchResponse(SearchItem[]? Items);
    private sealed record SearchItem(uint Id, string? Name, string? TinyImage);
    private sealed record AppResponse(bool Success, AppData? Data);
    private sealed record AppData(
        string? Type, uint SteamAppid, string? Name, string? ShortDescription, string? HeaderImage,
        string[]? Developers, string[]? Publishers, Genre[]? Genres, ReleaseDate? ReleaseDate, Platforms? Platforms,
        JsonElement PcRequirements, JsonElement LinuxRequirements);
    private sealed record Genre(string Description);
    private sealed record ReleaseDate(bool ComingSoon, string? Date);
    private sealed record Platforms(bool Linux);
}
