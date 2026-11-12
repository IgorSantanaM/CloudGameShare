using System.Globalization;
using CloudGameShare.BuildingBlocks.Domain.Exceptions;
using CloudGameShare.Catalog.Domain.Games.Enum;

namespace CloudGameShare.Catalog.Domain.Games;

public sealed record GameLaunchConfig
{
    public GameLauncher Launcher { get; private init; }
    public string AppId { get; private init; } = null!;
    public GameRuntime Runtime { get; private init; }
    public string? ProtonVersion { get; private init; }

    private GameLaunchConfig() { }

    public GameLaunchConfig(
        GameLauncher launcher,
        string appId,
        GameRuntime runtime,
        string? protonVersion = null)
    {
        if (!System.Enum.IsDefined(launcher))
            throw new DomainException("Invalid game launcher.");

        if (!System.Enum.IsDefined(runtime))
            throw new DomainException("Invalid game runtime.");

        if (string.IsNullOrWhiteSpace(appId))
            throw new DomainException("Game App ID is required.");

        appId = appId.Trim();

        if (launcher == GameLauncher.Steam &&
            (!uint.TryParse(
                appId,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var steamId) || steamId == 0))
        {
            throw new DomainException(
                "Steam App ID must be a positive numeric identifier.");
        }

        if (runtime != GameRuntime.Proton &&
            !string.IsNullOrWhiteSpace(protonVersion))
        {
            throw new DomainException(
                "Proton version is only valid for Proton games.");
        }

        if (launcher == GameLauncher.XboxPc &&
            runtime != GameRuntime.WindowsNative)
        {
            throw new DomainException(
                "Xbox PC games require a Windows runtime.");
        }

        Launcher = launcher;
        AppId = appId;
        Runtime = runtime;
        ProtonVersion = string.IsNullOrWhiteSpace(protonVersion)
            ? null
            : protonVersion.Trim();
    }
}
