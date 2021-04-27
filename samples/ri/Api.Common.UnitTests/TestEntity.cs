using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;

namespace Api.Common.UnitTests
{
    public class TestEntity : IPersistableEntity
    {
        private TestEntity(IReadOnlyDictionary<string, object> properties)
        {
            var id = properties.GetValueOrDefault<Identifier>(nameof(Id));
            Id = id.HasValue()
                ? id
                : null;
            LastPersistedAtUtc = properties.GetValueOrDefault<DateTime?>(nameof(LastPersistedAtUtc));
            APropertyValue = properties.GetValueOrDefault<string>(nameof(APropertyValue));
        }

        public string APropertyValue { get; private set; }

        public Identifier Id { get; private set; }

        public DateTime? LastPersistedAtUtc { get; private set; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool? IsDeleted { get; }

        public Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id},
                {nameof(LastPersistedAtUtc), LastPersistedAtUtc},
                {nameof(APropertyValue), APropertyValue}
            };
        }

        public static EntityFactory<TestEntity> Rehydrate()
        {
            return (identifier, container, properties) => new TestEntity(properties);
        }
    }
}