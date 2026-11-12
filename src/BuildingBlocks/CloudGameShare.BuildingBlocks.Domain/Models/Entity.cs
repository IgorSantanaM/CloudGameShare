 using CloudGameShare.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Text;

namespace CloudGameShare.Domain.Core.Models
{
    public abstract class Entity<TId> where TId : notnull
    {
        public TId? Id { get; protected set; }
        private List<Event<TId>> _domainEvents = new();
        public IReadOnlyCollection<Event<TId>> DomainEvents => _domainEvents.AsReadOnly();

        public override bool Equals(object? obj)
        {
            var comparteTo = obj as Entity<TId>;

            if(ReferenceEquals(this, comparteTo)) return true;
            if(ReferenceEquals(null, comparteTo)) return false;

            return EqualityComparer<TId>.Default.Equals(Id, comparteTo.Id);
        }

        public static bool operator ==(Entity<TId> a, Entity<TId> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity<TId> a, Entity<TId> b)
            => !(a == b);

        public override int GetHashCode() =>
           (GetType().GetHashCode() * 907) + Id!.GetHashCode();

        public override string ToString()
            => $"{GetType().Name} [Id={Id}]";
        public void ClearDomainEvents()
            => _domainEvents.Clear();    

        protected void AddDomainEvent(Event<TId> domainEvent)
            => _domainEvents.Add(domainEvent);

    }
}
