using CloudGameShare.Domain.Core.Events;

namespace CloudGameShare.Catalog.Domain.Games.Events
{
    public sealed record GamePublishedEvent(Guid GameId) : Event<Guid>(GameId);
}
