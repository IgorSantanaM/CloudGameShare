using CloudGameShare.Catalog.Application.Exceptions;
using CloudGameShare.Catalog.Domain.Games;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.UnpublishGame;

public sealed class UnpublishGameCommandHandler(IGameRepository repository) : IRequestHandler<UnpublishGameCommand>
{
    public async Task Handle(UnpublishGameCommand request, CancellationToken cancellationToken)
    {
        var game = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Game '{request.Id}' was not found.");

        game.Unpublish();
        await repository.UpdateEntityAsync(game, cancellationToken);
    }
}
