using Microsoft.EntityFrameworkCore;
using CloudGameShare.Catalog.Domain.Games;
using CloudGameShare.Catalog.Infra.Data.Contexts;
using CloudGameShare.BuildingBlocks.Domain.Models;

namespace CloudGameShare.Catalog.Infra.Data.Repositories;

public sealed class GameRepository(CloudGameShareCatalogContext context) : Repository<Game, Guid>(context), IGameRepository
{
    public Task<Game?> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking().SingleOrDefaultAsync(game => game.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Game>> GetAllAsync(
        string? name = null,
        bool? isPublished = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var search = name.Trim().ToLowerInvariant();
            query = query.Where(game => game.Name.ToLower().Contains(search));
        }

        if (isPublished is bool published)
            query = query.Where(game => game.IsPublished == published);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var offset = (long)(pageNumber - 1) * pageSize;

        var result = new PagedResult<Game>
        {
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        if (offset < totalCount)
        {
            result.Items = await query.OrderBy(game => game.Name)
                .ThenBy(game => game.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        return result;
    }
}
