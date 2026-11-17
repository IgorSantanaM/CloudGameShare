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
        [FromServices] IMediator mediator,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var ownerId = context.User.FindFirst("sub")!.Value;

        CreateSessionCommand command = new CreateSessionCommand(ownerId, request.GameId, idempotencyKey);

        var session = await mediator.Send(command, cancellationToken);

        context.Response.Headers.RetryAfter = "2";

        return TypedResults.AcceptedAtRoute(session, StatusRoute, new { id = session.Id });
    }

    private static async Task<Ok<SessionStatusDto>> GetStatusAsync(
        Guid id,
        [FromServices] IMediator mediator,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var ownerId = context.User.FindFirst("sub")!.Value;

        context.Response.Headers.CacheControl = "no-store";

        GetSessionStatusQuery query = new GetSessionStatusQuery(ownerId, id);
        var status = await mediator.Send(query, cancellationToken);

        return TypedResults.Ok(status);
    }

    private sealed record CreateSessionRequest(Guid GameId);
}
