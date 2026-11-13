using CloudGameShare.GamingSession.Domain.Sessions;

namespace CloudGameShare.GamingSession.Application.Services;

public interface ICatalogSessionClient
{
    Task<SessionLaunchSpec> GetGameForSessionAsync(Guid gameId, CancellationToken cancellationToken);
}
