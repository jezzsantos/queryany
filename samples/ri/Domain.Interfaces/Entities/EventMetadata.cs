using System;
using QueryAny.Primitives;
using ServiceStack.Text;

namespace Domain.Interfaces.Entities
{
    public class EventMetadata : SingleValueObjectBase<EventMetadata, string>
    {
        public EventMetadata(string fqn) : base(fqn)
        {
        }

        public string Fqn => Value;

        protected override string ToValue(string value)
        {
            return value;
        }

        public static ValueObjectFactory<EventMetadata> Instantiate()
        {
            return (property, container) => new EventMetadata(property);
        }
    }

    public static class EventMetadataExtensions
    {
        public static IChangeEvent CreateEventFromJson(this EventMetadata metadata, string eventId, string eventJson)
        {
            metadata.GuardAgainstNull(nameof(metadata));
            eventId.GuardAgainstNullOrEmpty(nameof(eventId));
            eventJson.GuardAgainstNullOrEmpty(nameof(eventJson));

            var typeName = metadata.Fqn;
            var eventType = Type.GetType(typeName);
            if (eventType == null)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize event '{eventId}'. The type: '{typeName}' cannot be found in this AppDomain and codebase. Perhaps the type {typeName} has been renamed or no longer exists?");
            }
            var eventData = eventJson;
            try
            {
                return (IChangeEvent) JsonSerializer.DeserializeFromString(eventData, eventType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize event '{eventId}' as type: '{eventType}'",
                    ex);
            }
        }
    }
}