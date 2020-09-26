using System;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;

namespace Domain.Interfaces.Entities
{
    public class EntityEvent : IIdentifiableEntity, IQueryableEntity
    {
        public DateTime? LastPersistedAtUtc { get; set; }

        public long Version { get; private set; }

        public string EntityType { get; private set; }

        public string EventType { get; private set; }

        public string Data { get; private set; }

        public EventMetadata Metadata { get; private set; }

        public string StreamName { get; private set; }

        public Identifier Id { get; private set; }

        public void SetIdentifier(IIdentifierFactory factory)
        {
            factory.GuardAgainstNull(nameof(factory));

            Id = factory.Create(this);
        }

        public void SetEvent(string streamName, string entityType, long version, IChangeEvent @event)
        {
            streamName.GuardAgainstNullOrEmpty(nameof(streamName));
            entityType.GuardAgainstNullOrEmpty(nameof(entityType));
            @event.GuardAgainstNull(nameof(@event));

            StreamName = streamName;
            Version = version;
            EntityType = entityType;
            EventType = @event.GetType().Name;
            Data = @event.ToJson();
            Metadata = new EventMetadata(@event.GetType().AssemblyQualifiedName);
        }

        public IChangeEvent ToEvent()
        {
            return Metadata.CreateEventFromJson(Id, Data);
        }
    }
}