using CloudGameShare.GamingSession.Domain.Sessions;
using CloudGameShare.GamingSession.Infra.Data.Mappings.Mongo;
using CloudGameShare.GamingSession.Infra.Data.Repositories.Mongo;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace CloudGameShare.GamingSession.Infra.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddGamingSessionData(this IServiceCollection services, string connectionString, string databaseName)
    {
        GamingSessionMap.Configure();

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddSingleton(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
        services.AddSingleton(provider => provider.GetRequiredService<IMongoDatabase>()
            .GetCollection<GameSession>("gaming_sessions").WithWriteConcern(WriteConcern.WMajority));
        services.AddSingleton<IGamingSessionRepository, GamingSessionRepository>();

        return services;
    }

    public static Task EnsureGamingSessionIndexesAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        var sessions = services.GetRequiredService<IMongoCollection<GameSession>>();
        var keys = Builders<GameSession>.IndexKeys.Ascending(session => session.NextActionAtUtc)
            .Ascending(session => session.LeaseUntilUtc);
        return sessions.Indexes.CreateOneAsync(new CreateIndexModel<GameSession>(keys), cancellationToken: cancellationToken);
    }
}
