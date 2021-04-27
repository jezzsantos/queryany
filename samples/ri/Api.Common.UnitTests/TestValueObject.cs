using System.Collections.Generic;
using Domain.Interfaces.Entities;

namespace Api.Common.UnitTests
{
    public class TestValueObject : ValueObjectBase<TestValueObject>
    {
        private TestValueObject(string property)
        {
            APropertyValue = property;
        }

        public string APropertyValue { get; }

        public static ValueObjectFactory<TestValueObject> Rehydrate()
        {
            return (property, container) => new TestValueObject(property);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {APropertyValue};
        }
    }
}