using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;

namespace Api.Common.UnitTests
{
    public class TestEntity : IPersistableEntity
    {
        private TestEntity(IIdentifierFactory idFactory)
        {
            Id = idFactory.Create(this);
        }

        public string APropertyValue { get; private set; }

        public Identifier Id { get; private set; }

        public DateTime? LastPersistedAtUtc { get; private set; }

        public Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id},
                {nameof(LastPersistedAtUtc), LastPersistedAtUtc},
                {nameof(APropertyValue), APropertyValue}
            };
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            var id = properties.GetValueOrDefault<Identifier>(nameof(Id));
            Id = id.HasValue()
                ? id
                : null;
            LastPersistedAtUtc = properties.GetValueOrDefault<DateTime?>(nameof(LastPersistedAtUtc));
            APropertyValue = properties.GetValueOrDefault<string>(nameof(APropertyValue));
        }

        public static EntityFactory<TestEntity> Instantiate()
        {
            return (identifier, container, rehydratingProperties) =>
                new TestEntity(container.Resolve<IIdentifierFactory>());
        }
    }
}