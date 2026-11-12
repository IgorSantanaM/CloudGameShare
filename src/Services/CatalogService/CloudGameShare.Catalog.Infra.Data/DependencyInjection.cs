using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CloudGameShare.BuildingBlocks.Infra.Interceptors;
using CloudGameShare.Catalog.Domain.Games;
using CloudGameShare.Catalog.Infra.Data.Contexts;
using CloudGameShare.Catalog.Infra.Data.Repositories;

namespace CloudGameShare.Catalog.Infra.Data;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCatalogData(
            Action<DbContextOptionsBuilder> configureDatabase)
        {
            services.AddScoped<PublishDomainEventsInterceptor>();
            services.AddDbContext<CloudGameShareCatalogContext>((provider, options) =>
            {
                configureDatabase(options);
                options.AddInterceptors(provider.GetRequiredService<PublishDomainEventsInterceptor>());
            });
            services.AddScoped<IGameRepository, GameRepository>();
            return services;
        }
    }
}
