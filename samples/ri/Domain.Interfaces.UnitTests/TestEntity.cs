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

        protected override void When(object @event)
        {
            //Not used in testing
        }

        public static EntityFactory<TestEntity> Instantiate()
        {
            return (hydratingProperties, container) => new TestEntity(container.Resolve<ILogger>(),
                new HydrationIdentifierFactory(hydratingProperties));
        }

        public class ChangeEvent
        {
            public string APropertyName { get; set; }
        }
    }

    public class TestAggregateRoot : AggregateRootBase
    {
        public TestAggregateRoot(ILogger logger, IIdentifierFactory idFactory)
            : base(logger, idFactory)
        {
            RaiseCreateEvent(new CreateEvent {APropertyName = "acreatedvalue"});
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

        protected override void When(object @event)
        {
            //Not used in testing
        }

        public static EntityFactory<TestAggregateRoot> Instantiate()
        {
            return (hydratingProperties, container) => new TestAggregateRoot(container.Resolve<ILogger>(),
                new HydrationIdentifierFactory(hydratingProperties));
        }

        public class CreateEvent
        {
            public string APropertyName { get; set; }
        }

        public class ChangeEvent
        {
            public string APropertyName { get; set; }
        }
    }
}