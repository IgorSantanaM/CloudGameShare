using CloudGameShare.Catalog.Domain.Games;
using CloudGameShare.Catalog.Domain.Games.Enum;

namespace CloudGameShare.Catalog.Application.Features.Games.DTOs;

public sealed record GameLaunchConfigDto(GameLauncher Launcher, string AppId, GameRuntime Runtime, string? ProtonVersion = null)
{
    internal GameLaunchConfig ToDomain() => new(Launcher, AppId, Runtime, ProtonVersion);

    internal static GameLaunchConfigDto From(GameLaunchConfig config)
        => new(config.Launcher, config.AppId, config.Runtime, config.ProtonVersion);
}
