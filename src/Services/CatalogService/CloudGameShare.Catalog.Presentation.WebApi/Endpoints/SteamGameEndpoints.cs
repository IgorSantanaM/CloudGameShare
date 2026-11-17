using CloudGameShare.Catalog.Application.Features.Games.Queries.GetSteamGameDetails;
using CloudGameShare.Catalog.Application.Features.Games.Queries.SearchSteamGames;
using CloudGameShare.Catalog.Presentation.WebApi.Endpoints.Internal;
using MediatR;

namespace CloudGameShare.Catalog.Presentation.WebApi.Endpoints;

internal sealed class SteamGameEndpoints : IEndpoint
{
    public static void DefineEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/games/steam").WithTags("Steam");

        group.MapGet("/search", async (string name, ISender sender, CancellationToken cancellationToken, string language = "english") =>
            TypedResults.Ok(await sender.Send(new SearchSteamGamesQuery(name, language), cancellationToken)));

        group.MapGet("/{appId}", async (uint appId, ISender sender, CancellationToken cancellationToken, string language = "english") =>
            TypedResults.Ok(await sender.Send(new GetSteamGameDetailsQuery(appId, language), cancellationToken)));
    }
}
