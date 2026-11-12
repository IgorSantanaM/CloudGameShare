using CloudGameShare.BuildingBlocks.Domain.Models;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Queries.GetAllGames;

public sealed record GetAllGamesQuery(
    string? Name = null,
    bool? IsPublished = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<GameSummaryDto>>;
