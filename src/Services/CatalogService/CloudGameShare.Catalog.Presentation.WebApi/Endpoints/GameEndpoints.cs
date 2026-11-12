using CloudGameShare.BuildingBlocks.Domain.Models;
using CloudGameShare.Catalog.Application.Features.Games.Commands.CreateGame;
using CloudGameShare.Catalog.Application.Features.Games.Commands.DeleteGame;
using CloudGameShare.Catalog.Application.Features.Games.Commands.PublishGame;
using CloudGameShare.Catalog.Application.Features.Games.Commands.UnpublishGame;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using CloudGameShare.Catalog.Application.Features.Games.Queries.GetAllGames;
using CloudGameShare.Catalog.Application.Features.Games.Queries.GetGameById;
using CloudGameShare.Catalog.Presentation.WebApi.Endpoints.Internal;
using CloudGameShare.Catalog.Presentation.WebApi.Endpoints.Requests;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CloudGameShare.Catalog.Presentation.WebApi.Endpoints;

internal sealed class GameEndpoints : IEndpoint
{
    private const string GetGameByIdRoute = "GetGameById";

    public static void DefineEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/games").WithTags("Games");

        group.MapPost("/create", CreateGameAsync);
        group.MapGet("/", GetAllGamesAsync);
        group.MapGet("/{id:guid}", GetGameByIdAsync).WithName(GetGameByIdRoute);
        group.MapPut("/{id:guid}", UpdateGameAsync);
        group.MapDelete("/{id:guid}", DeleteGameAsync);
        group.MapPost("/{id:guid}/publish", PublishGameAsync);
        group.MapPost("/{id:guid}/unpublish", UnpublishGameAsync);
    }

    private static async Task<CreatedAtRoute<CreatedGameResponse>> CreateGameAsync(
        [FromBody] CreateGameCommand command,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.CreatedAtRoute(new CreatedGameResponse(id), GetGameByIdRoute, new { id });
    }

    private static async Task<Ok<PagedResult<GameSummaryDto>>> GetAllGamesAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] string? name = null,
        [FromQuery] bool? isPublished = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllGamesQuery(name, isPublished, pageNumber, pageSize);
        return TypedResults.Ok(await sender.Send(query, cancellationToken));
    }

    private static async Task<Ok<GameDetailsDto>> GetGameByIdAsync(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await sender.Send(new GetGameByIdQuery(id), cancellationToken));
    }

    private static async Task<NoContent> UpdateGameAsync(
        Guid id,
        [FromBody] UpdateGameRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(request.ToCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteGameAsync(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteGameCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> PublishGameAsync(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new PublishGameCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> UnpublishGameAsync(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UnpublishGameCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private sealed record CreatedGameResponse(Guid Id);
}
