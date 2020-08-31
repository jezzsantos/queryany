using System.Collections.Generic;
using System.Linq;
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
        public EventStreamStateChangedArgs(string streamName, List<EventStreamStateChangeEvent> events)
        {
            streamName.GuardAgainstNull(nameof(streamName));
            events.GuardAgainstNull(nameof(events));
            StreamName = streamName;
            StartingAt = events.First().Version;
            Events = events;
        }

        public string StreamName { get; }

        public long StartingAt { get; }

        public List<EventStreamStateChangeEvent> Events { get; }
    }

    public class EventStreamStateChangeEvent
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Data { get; set; }

        public long Version { get; set; }
    }
}