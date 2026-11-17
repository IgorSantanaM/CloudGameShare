namespace CloudGameShare.Catalog.Application.Features.Games.DTOs;

public sealed record SteamGameSummaryDto(uint AppId, string Name, string? CoverImageUrl);
