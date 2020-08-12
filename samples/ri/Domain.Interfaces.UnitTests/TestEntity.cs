using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Domain.Interfaces.UnitTests
{
    public class TestEntity : EntityBase
    {
        public TestEntity(ILogger logger, IIdentifierFactory idFactory) : base(logger, idFactory)
        {
        }

        public string APropertyName { get; private set; }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            APropertyName = properties.GetValueOrDefault<string>(nameof(APropertyName));
        }

        public static EntityFactory<TestEntity> GetFactory()
        {
            return (hydratingProperties, container) => new TestEntity(container.Resolve<ILogger>(),
                new HydrationIdentifierFactory(hydratingProperties));
        }
    }
}