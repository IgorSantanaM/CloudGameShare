using CloudGameShare.Catalog.Contracts;
using CloudGameShare.GamingSession.Application.Exceptions;
using CloudGameShare.GamingSession.Application.Services;
using CloudGameShare.GamingSession.Domain.Sessions;
using Grpc.Core;

namespace CloudGameShare.GamingSession.Infra.Services.Catalog;

internal sealed class GrpcCatalogSessionClient(CatalogSessions.CatalogSessionsClient client) : ICatalogSessionClient
{
    public async Task<SessionLaunchSpec> GetGameForSessionAsync(Guid gameId, CancellationToken cancellationToken)
    {
        try
        {
            var game = await client.GetGameForSessionAsync(
                new GetGameForSessionRequest { GameId = gameId.ToString("D") },
                deadline: DateTime.UtcNow.AddSeconds(5), cancellationToken: cancellationToken);

            var runtime = game.Runtime switch
            {
                SteamRuntime.LinuxNative => SessionRuntime.LinuxNative,
                SteamRuntime.Proton => SessionRuntime.Proton,
                _ => throw new SessionRequestException(SessionRequestError.Conflict, "The game runtime is unsupported.")
            };
            return new SessionLaunchSpec(game.SteamAppId, runtime, game.HasProtonVersion ? game.ProtonVersion : null,
                game.CpuCores, game.MemoryGb, game.GpuCount, game.StorageGb);
        }
        catch (RpcException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw exception.StatusCode switch
            {
                StatusCode.NotFound => new SessionRequestException(SessionRequestError.NotFound, "Game was not found."),
                StatusCode.FailedPrecondition => new SessionRequestException(SessionRequestError.Conflict, exception.Status.Detail),
                _ => new SessionRequestException(SessionRequestError.Unavailable, "Catalog is temporarily unavailable.")
            };
        }
    }
}
