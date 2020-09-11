using System;
using Domain.Interfaces.Entities;

namespace Storage.Interfaces
{
    public class EventStreamStateChangeEvent
    {
        public string Id { get; set; }

        public DateTime LastPersistedAtUtc { get; set; }

        public string StreamName { get; set; }

        public string EntityType { get; set; }

        public string EventType { get; set; }

        public long Version { get; set; }

        public string Data { get; set; }

        public EventMetadata Metadata { get; set; }
    }
}