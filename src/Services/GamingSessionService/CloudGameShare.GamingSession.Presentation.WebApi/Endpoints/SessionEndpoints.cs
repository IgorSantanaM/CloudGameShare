using CloudGameShare.GamingSession.Application.Features.Sessions.Commands.CreateSession;
using CloudGameShare.GamingSession.Application.Features.Sessions.DTOs;
using CloudGameShare.GamingSession.Application.Features.Sessions.Queries.GetSessionStatus;
using CloudGameShare.GamingSession.Presentation.WebApi.Endpoints.Internal;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CloudGameShare.GamingSession.Presentation.WebApi.Endpoints;

internal sealed class SessionEndpoints : IEndpoint
{
    private const string StatusRoute = "GetGamingSessionStatus";

    public static void DefineEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/gaming/sessions")
            .WithTags("Gaming Sessions")
            .RequireAuthorization("SessionOwner");
        group.MapPost("/", CreateAsync);
        group.MapGet("/{id:guid}", GetStatusAsync).WithName(StatusRoute);
    }

    private static async Task<AcceptedAtRoute<SessionStatusDto>> CreateAsync(
        [FromBody] CreateSessionRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey,
        [FromServices] ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var ownerId = context.User.FindFirst("sub")!.Value;
        var session = await sender.Send(new CreateSessionCommand(ownerId, request.GameId, idempotencyKey), cancellationToken);
        context.Response.Headers.RetryAfter = "2";
        return TypedResults.AcceptedAtRoute(session, StatusRoute, new { id = session.Id });
    }

    private static async Task<Ok<SessionStatusDto>> GetStatusAsync(
        Guid id,
        [FromServices] ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var ownerId = context.User.FindFirst("sub")!.Value;
        context.Response.Headers.CacheControl = "no-store";
        return TypedResults.Ok(await sender.Send(new GetSessionStatusQuery(ownerId, id), cancellationToken));
    }

    private sealed record CreateSessionRequest(Guid GameId);
}
