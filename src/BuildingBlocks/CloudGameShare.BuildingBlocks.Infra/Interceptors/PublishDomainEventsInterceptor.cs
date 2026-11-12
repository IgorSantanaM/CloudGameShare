using CloudGameShare.Domain.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace CloudGameShare.BuildingBlocks.Infra.Interceptors
{
    public class PublishDomainEventsInterceptor(IPublisher mediator) : SaveChangesInterceptor
    {
        public async override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            await CollectAndPublishDomainEvents(eventData.Context, cancellationToken);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task CollectAndPublishDomainEvents(DbContext? context, CancellationToken cancellationToken)
        {
            if (context == null)
                return;

            var entitiesWithEvents = context.ChangeTracker
                .Entries<Entity<Guid>>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            if (entitiesWithEvents.Count == 0)
                return;

            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            foreach(var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent, cancellationToken);
            }

        }
    }
}
