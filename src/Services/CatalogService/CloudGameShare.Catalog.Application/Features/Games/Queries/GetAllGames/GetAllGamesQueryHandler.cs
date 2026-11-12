using CloudGameShare.BuildingBlocks.Domain.Models;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using CloudGameShare.Catalog.Domain.Games;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Queries.GetAllGames;

public sealed class GetAllGamesQueryHandler(IGameRepository repository)
    : IRequestHandler<GetAllGamesQuery, PagedResult<GameSummaryDto>>
{
    public async Task<PagedResult<GameSummaryDto>> Handle(GetAllGamesQuery request, CancellationToken cancellationToken)
    {
        var games = await repository.GetAllAsync(
            request.Name, request.IsPublished, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<GameSummaryDto>
        {
            Items = games.Items
                .Select(game => new GameSummaryDto(game.Id, game.Name, game.Genre, game.CoverImageUrl, game.IsPublished))
                .ToArray(),
            TotalCount = games.TotalCount,
            TotalPages = games.TotalPages
        };
    }
}
