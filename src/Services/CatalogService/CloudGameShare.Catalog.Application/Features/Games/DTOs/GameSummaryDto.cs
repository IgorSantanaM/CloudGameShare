namespace CloudGameShare.Catalog.Application.Features.Games.DTOs;

public sealed record GameSummaryDto(Guid Id, string Name, string Genre, string? CoverImageUrl, bool IsPublished);
