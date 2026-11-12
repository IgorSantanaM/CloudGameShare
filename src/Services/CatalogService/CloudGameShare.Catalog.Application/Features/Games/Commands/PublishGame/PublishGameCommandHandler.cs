using CloudGameShare.Catalog.Application.Exceptions;
using CloudGameShare.Catalog.Domain.Games;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.PublishGame;

public sealed class PublishGameCommandHandler(IGameRepository repository) : IRequestHandler<PublishGameCommand>
{
    public async Task Handle(PublishGameCommand request, CancellationToken cancellationToken)
    {
        var game = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Game '{request.Id}' was not found.");

        game.Publish();
        await repository.UpdateEntityAsync(game, cancellationToken);
    }
}
