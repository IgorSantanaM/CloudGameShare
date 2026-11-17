namespace CloudGameShare.Catalog.Application.Features.Games.DTOs;

public sealed record SteamGameDetailsDto(
    uint AppId,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string[] Genres,
    string[] Publishers,
    string[] Developers,
    string? CoverImageUrl,
    bool SupportsLinux);
