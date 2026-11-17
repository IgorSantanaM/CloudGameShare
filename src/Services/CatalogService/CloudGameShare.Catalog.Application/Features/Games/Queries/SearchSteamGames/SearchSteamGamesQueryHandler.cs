using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using CloudGameShare.Catalog.Application.Services;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Queries.SearchSteamGames;

public sealed class SearchSteamGamesQueryHandler(ISteamStoreClient client)
    : IRequestHandler<SearchSteamGamesQuery, IReadOnlyList<SteamGameSummaryDto>>
{
    public Task<IReadOnlyList<SteamGameSummaryDto>> Handle(SearchSteamGamesQuery request, CancellationToken cancellationToken)
        => client.SearchAsync(request.Name.Trim(), request.Language, cancellationToken);
}
