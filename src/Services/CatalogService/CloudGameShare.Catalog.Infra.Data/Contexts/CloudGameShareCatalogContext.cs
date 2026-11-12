using CloudGameShare.Catalog.Domain.Games;
using CloudGameShare.Domain.Core.Events;
using Microsoft.EntityFrameworkCore;

namespace CloudGameShare.Catalog.Infra.Data.Contexts
{
    public sealed class CloudGameShareCatalogContext(DbContextOptions<CloudGameShareCatalogContext> options) : DbContext(options)
    {
        public DbSet<Game> Games => Set<Game>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Event<Guid>>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CloudGameShareCatalogContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
