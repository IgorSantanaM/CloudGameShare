namespace CloudGameShare.Catalog.Application.Features.Games.DTOs;

public sealed record GameDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime ReleaseDate,
    string Genre,
    string Publisher,
    string Developer,
    string? CoverImageUrl,
    string? TrailerUrl,
    string? GameUrl,
    bool IsPublished,
    GameRequirementsDto Requirements,
    GameLaunchConfigDto LaunchConfig);
