using Microsoft.EntityFrameworkCore;
using CloudGameShare.Catalog.Infra.Data.Contexts;
using CloudGameShare.Domain.Core.Data;
using CloudGameShare.Domain.Core.Models;

namespace CloudGameShare.Catalog.Infra.Data.Repositories;

public abstract class Repository<TEntity, TId>(CloudGameShareCatalogContext context) : IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    protected DbSet<TEntity> DbSet { get; } = context.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return DbSet.FindAsync([id], cancellationToken).AsTask();
    }

    public async Task<TId> CreateEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entity.Id!;
    }

    public async Task UpdateEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEntityAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return;

        DbSet.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
