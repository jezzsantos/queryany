using System;
using QueryAny.Primitives;
using ServiceStack;
using ServiceStack.Text;

namespace Domain.Interfaces.Entities
{
    public class EntityEvent : DataEntity
    {
        public long Version
        {
            get => this.PropertyValues.GetValueOrDefault<long>(nameof(Version));
            set => this.PropertyValues[nameof(Version)] = value;
        }

        public string EntityType
        {
            get => this.PropertyValues.GetValueOrDefault<string>(nameof(EntityType));
            set => this.PropertyValues[nameof(EntityType)] = value;
        }

        public string EventType
        {
            get => this.PropertyValues.GetValueOrDefault<string>(nameof(EventType));
            set => this.PropertyValues[nameof(EventType)] = value;
        }

        public string Data
        {
            get => this.PropertyValues.GetValueOrDefault<string>(nameof(Data));
            set => this.PropertyValues[nameof(Data)] = value;
        }

        public EventMetadata Metadata
        {
            get => this.PropertyValues.GetValueOrDefault<EventMetadata>(nameof(Metadata));
            set => this.PropertyValues[nameof(Metadata)] = value;
        }

        public string StreamName
        {
            get => this.PropertyValues.GetValueOrDefault<string>(nameof(StreamName));
            set => this.PropertyValues[nameof(StreamName)] = value;
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