using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Queries.GetGameById;

public sealed record GetGameByIdQuery(Guid Id) : IRequest<GameDetailsDto>;
