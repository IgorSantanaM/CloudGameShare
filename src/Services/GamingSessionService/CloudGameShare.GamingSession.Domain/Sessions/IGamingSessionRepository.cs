namespace CloudGameShare.GamingSession.Domain.Sessions;

public interface IGamingSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid id, string ownerId, CancellationToken cancellationToken);
    Task<GameSession> AddAsync(GameSession session, CancellationToken cancellationToken);
    Task<GameSession?> ClaimNextAsync(DateTime now, TimeSpan leaseDuration, CancellationToken cancellationToken);
    Task CompleteClaimAsync(GameSession session, CancellationToken cancellationToken);
}
