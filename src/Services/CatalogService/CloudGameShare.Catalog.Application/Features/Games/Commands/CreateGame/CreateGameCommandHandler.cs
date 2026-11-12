using CloudGameShare.Catalog.Domain.Games;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.CreateGame;

public sealed class CreateGameCommandHandler(IGameRepository repository) : IRequestHandler<CreateGameCommand, Guid>
{
    public Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var game = new Game(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.ReleaseDate,
            request.Genre,
            request.Publisher,
            request.Developer,
            request.Requirements.ToDomain(),
            request.LaunchConfig.ToDomain(),
            request.CoverImageUrl,
            request.TrailerUrl,
            request.GameUrl);

        return repository.CreateEntityAsync(game, cancellationToken);
    }
}
