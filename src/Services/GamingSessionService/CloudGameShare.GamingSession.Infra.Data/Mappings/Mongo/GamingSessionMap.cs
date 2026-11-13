using CloudGameShare.GamingSession.Domain.Sessions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace CloudGameShare.GamingSession.Infra.Data.Mappings.Mongo;

internal static class GamingSessionMap
{
    public static void Configure()
    {
        BsonClassMap.TryRegisterClassMap<GameSession>(map =>
        {
            map.AutoMap();
            map.SetIgnoreExtraElements(true);
            map.MapIdMember(session => session.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            map.MapMember(session => session.GameId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
        });
    }
}
