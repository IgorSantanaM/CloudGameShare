using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.PublishGame;

public sealed record PublishGameCommand(Guid Id) : IRequest;
