using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace Storage.Interfaces
{
    public interface IEventingStorage<TAggregateRoot>
        where TAggregateRoot : IPersistableAggregateRoot
    {
        TAggregateRoot Load(Identifier id);

        void Save(TAggregateRoot aggregate);

        event EventStreamStateChanged OnEventStreamStateChanged;

        void DestroyAll();
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