using System;
using System.Collections.Generic;
using QueryAny.Primitives;
using ServiceStack;
using ServiceStack.Text;

namespace Domain.Interfaces.Entities
{
    public class EventEntity : IPersistableEntity
    {
        public EventEntity(IIdentifierFactory idFactory)
        {
            Id = idFactory.Create(this);
        }

        public long Version { get; private set; }

        public string TypeName { get; private set; }

        public string Data { get; private set; }

        public EventMetadata Metadata { get; private set; }

        public string StreamName { get; private set; }

        public Identifier Id { get; private set; }

        public DateTime? LastPersistedAtUtc { get; private set; }

        public Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id},
                {nameof(LastPersistedAtUtc), LastPersistedAtUtc},
                {nameof(StreamName), StreamName},
                {nameof(Version), Version},
                {nameof(TypeName), TypeName},
                {nameof(Data), Data},
                {nameof(Metadata), Metadata}
            };
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            var id = properties.GetValueOrDefault<Identifier>(nameof(Id));
            Id = id.HasValue()
                ? id
                : null;
            LastPersistedAtUtc = properties.GetValueOrDefault<DateTime?>(nameof(LastPersistedAtUtc));
            StreamName = properties.GetValueOrDefault<string>(nameof(StreamName));
            Version = properties.GetValueOrDefault<long>(nameof(Version));
            TypeName = properties.GetValueOrDefault<string>(nameof(TypeName));
            Data = properties.GetValueOrDefault<string>(nameof(Data));
            Metadata = properties.GetValueOrDefault<EventMetadata>(nameof(Metadata));
        }

        public void SetEvent(string streamName, long version, object @event)
        {
            streamName.GuardAgainstNullOrEmpty(nameof(streamName));
            @event.GuardAgainstNull(nameof(@event));

            StreamName = streamName;
            Version = version;
            TypeName = @event.GetType().Name;
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

        public static EntityFactory<EventEntity> Instantiate()
        {
            return (identifier, container) => new EventEntity(container.Resolve<IIdentifierFactory>());
        }
    }
}