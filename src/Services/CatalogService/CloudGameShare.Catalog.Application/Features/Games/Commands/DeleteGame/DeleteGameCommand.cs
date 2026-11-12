using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.DeleteGame;

public sealed record DeleteGameCommand(Guid Id) : IRequest;
