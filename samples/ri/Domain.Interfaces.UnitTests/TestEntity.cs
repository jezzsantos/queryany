using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Domain.Interfaces.UnitTests
{
    public class TestEntity : EntityBase
    {
        public TestEntity(ILogger logger, IIdentifierFactory idFactory)
            : base(logger, idFactory)
        {
        }

        private TestEntity(ILogger logger, IIdentifierFactory idFactory, Identifier identifier)
            : base(logger, idFactory, identifier)
        {
        }

        public string APropertyName { get; private set; }

        public void ChangeProperty(string value)
        {
            RaiseChangeEvent(new ChangeEvent {APropertyName = value});
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            APropertyName = properties.GetValueOrDefault<string>(nameof(APropertyName));
        }

        protected override void OnEventRaised(IChangeEvent @event)
        {
            //Not used in testing
        }

        public static EntityFactory<TestEntity> Instantiate()
        {
            return (identifier, container, rehydratingProperties) => new TestEntity(container.Resolve<ILogger>(),
                container.Resolve<IIdentifierFactory>(), identifier);
        }

        public class ChangeEvent : IChangeEvent
        {
            public string APropertyName { get; set; }

            public string Id { get; set; }

            public DateTime ModifiedUtc { get; set; }
        }
    }

    public class NullIdentifierFactory : IIdentifierFactory
    {
        public Identifier Create(IIdentifiableEntity entity)
        {
            return null;
        }

        public bool IsValid(Identifier value)
        {
            throw new NotImplementedException();
        }
    }
}