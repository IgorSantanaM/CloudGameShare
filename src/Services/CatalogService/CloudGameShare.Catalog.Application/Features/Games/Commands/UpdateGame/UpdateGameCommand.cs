using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Commands.UpdateGame;

public sealed record UpdateGameCommand(
    Guid Id,
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
    string? GameUrl = null) : IRequest;
