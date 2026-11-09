using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace CloudGameShare.Domain.Core.Events
{
    public abstract record Message<TId> where TId : notnull
    {
        public string MessageType { get; private set; }
        public TId? AggregateId { get; private set; }

        protected Message(TId aggregateId)
        {
            AggregateId = aggregateId;
            MessageType = GetType().Name;   
        }
    }
}
