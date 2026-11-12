using CloudGameShare.Catalog.Application.Exceptions;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using CloudGameShare.Catalog.Domain.Games;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Queries.GetGameById;

public sealed class GetGameByIdQueryHandler(IGameRepository repository) : IRequestHandler<GetGameByIdQuery, GameDetailsDto>
{
    public async Task<GameDetailsDto> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
    {
        var game = await repository.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Game '{request.Id}' was not found.");

        return new GameDetailsDto(game.Id, game.Name, game.Description, game.ReleaseDate, game.Genre,
            game.Publisher, game.Developer, game.CoverImageUrl, game.TrailerUrl, game.GameUrl, game.IsPublished,
            GameRequirementsDto.From(game.Requirements), GameLaunchConfigDto.From(game.LaunchConfig));
    }
}
