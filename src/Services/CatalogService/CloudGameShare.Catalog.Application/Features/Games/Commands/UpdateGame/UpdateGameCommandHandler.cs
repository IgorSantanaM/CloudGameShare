using CloudGameShare.Catalog.Application.Exceptions;
using CloudGameShare.Catalog.Domain.Games;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.UpdateGame;

public sealed class UpdateGameCommandHandler(IGameRepository repository) : IRequestHandler<UpdateGameCommand>
{
    public async Task Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Game '{request.Id}' was not found.");

        var requirements = request.Requirements.ToDomain();
        var launchConfig = request.LaunchConfig.ToDomain();

        game.UpdateMetadata(request.Name, request.Description, request.ReleaseDate, request.Genre,
            request.Publisher, request.Developer, request.CoverImageUrl, request.TrailerUrl, request.GameUrl);
        game.UpdateRequirements(requirements);
        game.UpdateLaunchConfig(launchConfig);

        await repository.UpdateEntityAsync(game, cancellationToken);
    }
}
