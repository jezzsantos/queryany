using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Domain.Interfaces.UnitTests
{
    public class TestAggregateRoot : AggregateRootBase
    {
        public TestAggregateRoot(ILogger logger, IIdentifierFactory idFactory)
            : base(logger, idFactory, id => new CreateEvent {Id = id})
        {
        }

        private TestAggregateRoot(ILogger logger, IIdentifierFactory idFactory, Identifier identifier)
            : base(logger, idFactory, identifier)
        {
        }

        public new long ChangeVersion => base.ChangeVersion;

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

        protected override void OnStateChanged(IChangeEvent @event)
        {
            //Not used in testing
        }

        public static EntityFactory<TestAggregateRoot> Instantiate()
        {
            return (identifier, container, rehydratingProperties) => new TestAggregateRoot(container.Resolve<ILogger>(),
                container.Resolve<IIdentifierFactory>(), identifier);
        }

        public class CreateEvent : IChangeEvent
        {
            public string Id { get; set; }

            public DateTime ModifiedUtc { get; set; }
        }

        public class ChangeEvent : IChangeEvent
        {
            public string APropertyName { get; set; }

            public string Id { get; set; }

            public DateTime ModifiedUtc { get; set; }
        }
    }
}