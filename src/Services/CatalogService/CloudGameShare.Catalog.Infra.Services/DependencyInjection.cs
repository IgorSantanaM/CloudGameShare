using CloudGameShare.Catalog.Application.Services;
using CloudGameShare.Catalog.Infra.Services.Steam;
using Microsoft.Extensions.DependencyInjection;

namespace CloudGameShare.Catalog.Infra.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogServices(this IServiceCollection services)
    {
        services.AddMemoryCache(options => options.SizeLimit = 512);
        services.AddHttpClient<ISteamStoreClient, SteamStoreClient>(client =>
        {
            client.BaseAddress = new Uri("https://store.steampowered.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CloudGameShare/1.0");
        });
        return services;
    }
}
