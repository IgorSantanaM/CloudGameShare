using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.UnpublishGame;

public sealed record UnpublishGameCommand(Guid Id) : IRequest;
