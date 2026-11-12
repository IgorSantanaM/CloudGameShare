using CloudGameShare.Catalog.Domain.Games;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.DeleteGame;

public sealed class DeleteGameCommandHandler(IGameRepository repository) : IRequestHandler<DeleteGameCommand>
{
    public Task Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        return repository.DeleteEntityAsync(request.Id, cancellationToken);
    }
}
