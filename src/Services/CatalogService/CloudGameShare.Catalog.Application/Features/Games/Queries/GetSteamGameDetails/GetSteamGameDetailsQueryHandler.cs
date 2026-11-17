using CloudGameShare.Catalog.Application.Exceptions;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using CloudGameShare.Catalog.Application.Services;
using MediatR;

namespace CloudGameShare.Catalog.Application.Features.Games.Queries.GetSteamGameDetails;

public sealed class GetSteamGameDetailsQueryHandler(ISteamStoreClient client)
    : IRequestHandler<GetSteamGameDetailsQuery, SteamGameDetailsDto>
{
    public async Task<SteamGameDetailsDto> Handle(GetSteamGameDetailsQuery request, CancellationToken cancellationToken)
        => await client.GetDetailsAsync(request.AppId, request.Language, cancellationToken)
            ?? throw new NotFoundException($"Steam game '{request.AppId}' was not found.");
}
