﻿using System;
using Common;

namespace Domain.Interfaces.Entities
{
    public class EventMetadata : SingleValueObjectBase<EventMetadata, string>
    {
        public EventMetadata(string fqn) : base(fqn)
        {
        }

        public string Fqn => Value;

        public static ValueObjectFactory<EventMetadata> Rehydrate()
        {
            return (property, container) => new EventMetadata(property);
        }
    }

    public static class EventMetadataExtensions
    {
        public static IChangeEvent CreateEventFromJson(this EventMetadata metadata, string eventId, string eventJson,
            IChangeEventMigrator migrator)
        {
            metadata.GuardAgainstNull(nameof(metadata));
            eventId.GuardAgainstNullOrEmpty(nameof(eventId));
            eventJson.GuardAgainstNullOrEmpty(nameof(eventJson));

            var typeName = metadata.Fqn;
            var eventData = eventJson;
            try
            {
                return migrator.Rehydrate(eventId, eventData, typeName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize event '{eventId}' as type: '{typeName}'",
                    ex);
            }
        }
    }
}