using CloudGameShare.GamingSession.Domain.Sessions;
using MongoDB.Driver;

namespace CloudGameShare.GamingSession.Infra.Data.Repositories.Mongo;

internal sealed class GamingSessionRepository(IMongoCollection<GameSession> sessions) : IGamingSessionRepository
{
    private static readonly FilterDefinitionBuilder<GameSession> Filter = Builders<GameSession>.Filter;

    public async Task<GameSession?> GetByIdAsync(Guid id, string ownerId, CancellationToken cancellationToken)
        => await sessions.Find(session => session.Id == id && session.OwnerId == ownerId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<GameSession> AddAsync(GameSession session, CancellationToken cancellationToken)
    {
        try
        {
            await sessions.InsertOneAsync(session, cancellationToken: cancellationToken);
            return session;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            var existing = await GetByIdAsync(session.Id, session.OwnerId, cancellationToken);
            if (existing is null)
                throw;

            return existing;
        }
    }

    public async Task<GameSession?> ClaimNextAsync(DateTime now, TimeSpan leaseDuration, CancellationToken cancellationToken)
    {
        var eligible = Filter.Lte(session => session.NextActionAtUtc, now)
            & (Filter.Eq(session => session.LeaseUntilUtc, null) | Filter.Lte(session => session.LeaseUntilUtc, now));
        var lease = Builders<GameSession>.Update
            .Set(session => session.LeaseId, Guid.NewGuid().ToString("N"))
            .Set(session => session.LeaseUntilUtc, now.Add(leaseDuration));

        return await sessions.FindOneAndUpdateAsync(eligible, lease, new FindOneAndUpdateOptions<GameSession>
        {
            Sort = Builders<GameSession>.Sort.Ascending(session => session.NextActionAtUtc),
            ReturnDocument = ReturnDocument.After
        }, cancellationToken);
    }

    public Task CompleteClaimAsync(GameSession session, CancellationToken cancellationToken)
    {
        var leaseId = session.LeaseId ?? throw new InvalidOperationException("The session has no provisioning lease.");
        return sessions.ReplaceOneAsync(
            current => current.Id == session.Id && current.LeaseId == leaseId,
            session with { LeaseId = null, LeaseUntilUtc = null },
            cancellationToken: cancellationToken);
    }
}
