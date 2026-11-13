using CloudGameShare.Catalog.Application.Exceptions;
using CloudGameShare.Catalog.Application.Features.Games.Queries.GetGameById;
using CloudGameShare.Catalog.Contracts;
using CloudGameShare.Catalog.Domain.Games.Enum;
using Grpc.Core;
using MediatR;

namespace CloudGameShare.Catalog.Presentation.WebApi.Grpc;

public sealed class CatalogSessionsService(ISender sender) : CatalogSessions.CatalogSessionsBase
{
    public override async Task<GameForSessionResponse> GetGameForSession(GetGameForSessionRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.GameId, out var gameId) || gameId == Guid.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Game ID must be a nonempty GUID."));

        try
        {
            var game = await sender.Send(new GetGameByIdQuery(gameId), context.CancellationToken);
            if (!game.IsPublished)
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "The game is not published."));

            var runtime = game.LaunchConfig.Runtime switch
            {
                GameRuntime.LinuxNative => SteamRuntime.LinuxNative,
                GameRuntime.Proton => SteamRuntime.Proton,
                _ => SteamRuntime.Unspecified
            };
            if (game.LaunchConfig.Launcher != GameLauncher.Steam || runtime == SteamRuntime.Unspecified)
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "This launcher or runtime is not supported for game sessions."));

            var response = new GameForSessionResponse
            {
                SteamAppId = game.LaunchConfig.AppId,
                Runtime = runtime,
                CpuCores = game.Requirements.CpuCores,
                MemoryGb = game.Requirements.MemoryGb,
                GpuCount = game.Requirements.GpuCount,
                StorageGb = game.Requirements.StorageGb
            };
            if (game.LaunchConfig.ProtonVersion is { } protonVersion)
                response.ProtonVersion = protonVersion;

            return response;
        }
        catch (NotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Game was not found."));
        }
    }
}
