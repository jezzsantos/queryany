using System;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using ServiceStack.Text;

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

        public void SetEvent(string streamName, string entityType, long version, object @event)
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

        public object ToEvent()
        {
            var eventType = Type.GetType(Metadata.Fqn);
            if (eventType == null)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize event '{Id}', the type: '{Metadata.Fqn}' cannot be found in this codebase. Perhaps it has been renamed or deleted?");
            }
            var eventData = Data;
            try
            {
                return JsonSerializer.DeserializeFromString(eventData, eventType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize event '{Id}' as type: '{eventType}'", ex);
            }
        }
    }
}