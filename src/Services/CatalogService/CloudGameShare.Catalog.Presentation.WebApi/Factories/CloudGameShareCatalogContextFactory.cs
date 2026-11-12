using CloudGameShare.Catalog.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CloudGameShare.Catalog.Presentation.WebApi.Factories;

public sealed class CloudGameShareCatalogContextFactory : IDesignTimeDbContextFactory<CloudGameShareCatalogContext>
{
    public CloudGameShareCatalogContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environments.Development;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true);

        if (environment.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase))
            configuration.AddUserSecrets<CloudGameShareCatalogContextFactory>(optional: true);

        var connectionString = configuration.AddEnvironmentVariables().AddCommandLine(args).Build()
            .GetConnectionString("Catalog")
            ?? throw new InvalidOperationException("Configure the Catalog connection string.");

        var options = new DbContextOptionsBuilder<CloudGameShareCatalogContext>().UseNpgsql(connectionString);
        return new CloudGameShareCatalogContext(options.Options);
    }
}
