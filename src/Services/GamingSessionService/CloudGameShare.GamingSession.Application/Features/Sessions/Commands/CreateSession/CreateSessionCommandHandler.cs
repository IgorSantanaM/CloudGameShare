using System.Security.Cryptography;
using System.Text;
using CloudGameShare.GamingSession.Application.Exceptions;
using CloudGameShare.GamingSession.Application.Features.Sessions.DTOs;
using CloudGameShare.GamingSession.Application.Services;
using CloudGameShare.GamingSession.Domain.Sessions;
using MediatR;

namespace CloudGameShare.GamingSession.Application.Features.Sessions.Commands.CreateSession;

public sealed class CreateSessionCommandHandler(
    IGamingSessionRepository repository,
    ICatalogSessionClient catalog,
    SessionSettings settings,
    TimeProvider timeProvider) : IRequestHandler<CreateSessionCommand, SessionStatusDto>
{
    public async Task<SessionStatusDto> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        if (request.IdempotencyKey is Guid key)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{request.OwnerId}:{key:D}"));
            id = new Guid(hash.AsSpan(0, 16));

            var existing = await repository.GetByIdAsync(id, request.OwnerId, cancellationToken);
            if (existing is not null)
                return CheckRequest(existing, request);
        }

        if (!settings.ProvisioningEnabled)
            throw new SessionRequestException(SessionRequestError.Unavailable, "Game provisioning is unavailable.");

        var launchSpec = await catalog.GetGameForSessionAsync(request.GameId, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var session = new GameSession(id, request.OwnerId, request.GameId, launchSpec, now,
            now.Add(settings.ProvisioningTimeout), now.Add(settings.SessionLifetime));

        var saved = await repository.AddAsync(session, cancellationToken);
        return CheckRequest(saved, request);
    }

    private static SessionStatusDto CheckRequest(GameSession session, CreateSessionCommand request)
    {
        if (session.GameId != request.GameId)
            throw new SessionRequestException(SessionRequestError.Conflict, "This idempotency key was used for a different game.");

        return SessionStatusDto.From(session);
    }
}
