using CloudGameShare.Catalog.Application.Features.Games.DTOs;

namespace CloudGameShare.Catalog.Application.Services;

public interface ISteamStoreClient
{
    Task<IReadOnlyList<SteamGameSummaryDto>> SearchAsync(string name, string language, CancellationToken cancellationToken);
    Task<SteamGameDetailsDto?> GetDetailsAsync(uint appId, string language, CancellationToken cancellationToken);
}
