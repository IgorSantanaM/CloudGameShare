using System;
using CloudGameShare.BuildingBlocks.Domain.Models;
using CloudGameShare.Domain.Core.Data;

namespace CloudGameShare.Catalog.Domain.Games
{
    public interface IGameRepository : IRepository<Game, Guid>
    {
        Task<Game?> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<Game>> GetAllAsync(
            string? name = null,
            bool? isPublished = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}
