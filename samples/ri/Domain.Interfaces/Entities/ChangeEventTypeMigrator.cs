using System;
using System.Collections.Generic;
using Common;
using Domain.Interfaces.Properties;
using ServiceStack;
using ServiceStack.Text;

namespace Domain.Interfaces.Entities
{
    public class ChangeEventTypeMigrator : IChangeEventMigrator
    {
        private readonly Dictionary<string, string> typeNameMappings;

        public ChangeEventTypeMigrator() : this(null)
        {
        }

        public ChangeEventTypeMigrator(Dictionary<string, string> typeNameMappings)
        {
            this.typeNameMappings = typeNameMappings;
        }

        public IChangeEvent Rehydrate(string eventId, string eventJson, string originalEventTypeName)
        {
            var migratedTypeName = originalEventTypeName;
            if (this.typeNameMappings != null)
            {
                if (this.typeNameMappings.ContainsKey(migratedTypeName))
                {
                    migratedTypeName = this.typeNameMappings[originalEventTypeName];
                }
            }

            var eventType = Type.GetType(migratedTypeName);
            if (eventType.NotExists())
            {
                throw new InvalidOperationException(
                    Resources.ChangeEventMigrator_UnknownType.Fmt(eventId, originalEventTypeName));
            }

            return (IChangeEvent) JsonSerializer.DeserializeFromString(eventJson, eventType);
        }
    }
}