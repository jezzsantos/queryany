using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace Storage.Interfaces
{
    public interface IEventStreamStorage<TAggregateRoot> : IEventPublishingStorage
        where TAggregateRoot : IPersistableAggregateRoot
    {
        TAggregateRoot Load(Identifier id);

        void Save(TAggregateRoot aggregate);

        void DestroyAll();
    }

    public interface IEventPublishingStorage
    {
        event EventStreamStateChanged OnEventStreamStateChanged;
    }

    public delegate void EventStreamStateChanged(object sender, EventStreamStateChangedArgs args);

    public class EventStreamStateChangedArgs
    {
        public EventStreamStateChangedArgs(List<EventStreamStateChangeEvent> events)
        {
            events.GuardAgainstNull(nameof(events));
            Events = events;
        }

        public List<EventStreamStateChangeEvent> Events { get; }
    }
}