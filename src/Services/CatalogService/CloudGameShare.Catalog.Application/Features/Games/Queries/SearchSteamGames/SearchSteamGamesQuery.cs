using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Queries.SearchSteamGames;

public sealed record SearchSteamGamesQuery(string Name, string Language = "english") : IRequest<IReadOnlyList<SteamGameSummaryDto>>;
