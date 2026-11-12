using CloudGameShare.Catalog.Application.Features.Games.Commands.UpdateGame;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;

namespace CloudGameShare.Catalog.Presentation.WebApi.Endpoints.Requests;

internal sealed record UpdateGameRequest(
    string Name,
    string? Description,
    DateTime ReleaseDate,
    string Genre,
    string Publisher,
    string Developer,
    GameRequirementsDto Requirements,
    GameLaunchConfigDto LaunchConfig,
    string? CoverImageUrl = null,
    string? TrailerUrl = null,
    string? GameUrl = null)
{
    public UpdateGameCommand ToCommand(Guid id) => new(
        id, Name, Description, ReleaseDate, Genre, Publisher, Developer, Requirements,
        LaunchConfig, CoverImageUrl, TrailerUrl, GameUrl);
}
