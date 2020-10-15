using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Api.Common.UnitTests
{
    public class TestValueObject : ValueObjectBase<TestValueObject>
    {
        // ReSharper disable once UnusedParameter.Local
        private TestValueObject(ILogger logger)
        {
            APropertyValue = null;
        }

        public string APropertyValue { get; private set; }

        public override void Rehydrate(string value)
        {
            APropertyValue = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {APropertyValue};
        }

        public static ValueObjectFactory<TestValueObject> Instantiate()
        {
            return (hydratingProperty, container) => new TestValueObject(container.Resolve<ILogger>());
        }
    }
}