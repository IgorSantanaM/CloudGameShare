using CloudGameShare.GamingSession.Domain.Sessions;

namespace CloudGameShare.GamingSession.Application.Features.Sessions.DTOs;

public sealed record SessionStatusDto(
    Guid Id,
    Guid GameId,
    SessionStatus Status,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    DateTime? ReadyAtUtc,
    DateTime? EndedAtUtc,
    string? FailureReason)
{
    internal static SessionStatusDto From(GameSession session)
        => new(session.Id, session.GameId, session.Status, session.CreatedAtUtc, session.ExpiresAtUtc,
            session.ReadyAtUtc, session.EndedAtUtc, session.FailureReason);
}
