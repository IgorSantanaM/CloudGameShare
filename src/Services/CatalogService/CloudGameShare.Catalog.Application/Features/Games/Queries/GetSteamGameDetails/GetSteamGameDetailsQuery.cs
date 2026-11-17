using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Queries.GetSteamGameDetails;

public sealed record GetSteamGameDetailsQuery(uint AppId, string Language = "english") : IRequest<SteamGameDetailsDto>;
