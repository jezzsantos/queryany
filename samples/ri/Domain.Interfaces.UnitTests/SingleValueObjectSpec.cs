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
            string stringValue = this.valueObject;

            stringValue.Should().Be("avalue");
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