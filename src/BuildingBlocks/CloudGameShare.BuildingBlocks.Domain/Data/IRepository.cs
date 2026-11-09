using CloudGameShare.Domain.Core.Models;

namespace CloudGameShare.Domain.Core.Data
{
    public interface IRepository<TEntity, TId> where TEntity : IAggregateRoot where TId : notnull
    {
        Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<TId> CreateEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteEntityAsync(TId id, CancellationToken cancellationToken = default);  
    }
}
