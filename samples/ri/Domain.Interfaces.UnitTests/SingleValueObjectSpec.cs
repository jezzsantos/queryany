using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class SingleValueObjectSpec
    {
        private readonly TestSingleStringValueObject valueObject;

        public SingleValueObjectSpec()
        {
            this.valueObject = new TestSingleStringValueObject("avalue");
        }

        [TestMethod]
        public void WhenAssignInstanceToString_ThenValueAssigned()
        {
            var stringValue = new TestSingleStringValueObject("avalue");

            stringValue.StringValue.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenAssignInstanceToEnumThenValueAssigned()
        {
            var enumValue = new TestSingleEnumValueObject(TestEnum.AValue1);

            enumValue.EnumValue.Should().Be(TestEnum.AValue1);
        }

        [TestMethod]
        public void WhenEquateWithSameValueObject_ThenReturnsTrue()
        {
            var result = this.valueObject == new TestSingleStringValueObject("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEquateWithDifferentValueObject_ThenReturnsFalse()
        {
            var result = this.valueObject == new TestSingleStringValueObject("anothervalue");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEquateWithSameStringValue_ThenReturnsTrue()
        {
            var result = this.valueObject == "avalue";

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEquateWithDifferentStringValue_ThenReturnsFalse()
        {
            var result = this.valueObject == "anothervalue";

            result.Should().BeFalse();
        }
    }
}