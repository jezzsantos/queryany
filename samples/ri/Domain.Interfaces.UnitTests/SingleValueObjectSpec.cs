using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class SingleValueObjectSpec
    {
        private TestSingleStringValueObject valueObject;

        [TestInitialize]
        public void Initialize()
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
            var enumValue = new TestSingleEnumValueObject(AnEnum.AValue1);

            enumValue.EnumValue.Should().Be(AnEnum.AValue1);
        }

        [TestMethod]
        public void WhenRehydrateStringThenValueAssigned()
        {
            var stringValue = new TestSingleStringValueObject("avalue");
            var dehydrated = stringValue.Dehydrate();

            stringValue.Rehydrate(dehydrated);

            stringValue.StringValue.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenRehydrateEnumThenValueAssigned()
        {
            var enumValue = new TestSingleEnumValueObject(AnEnum.AValue1);
            var dehydrated = enumValue.Dehydrate();

            enumValue.Rehydrate(dehydrated);

            enumValue.EnumValue.Should().Be(AnEnum.AValue1);
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