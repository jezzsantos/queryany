using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Domain.Interfaces.UnitTests
{
    public class TestEntity : EntityBase
    {
        public TestEntity(ILogger logger, IIdentifierFactory idFactory) : base(logger, idFactory)
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

        public static EntityFactory<TestEntity> Instantiate()
        {
            return (hydratingProperties, container) => new TestEntity(container.Resolve<ILogger>(),
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