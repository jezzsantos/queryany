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
            var stringValue = (string) this.valueObject;

            stringValue.Should().Be("avalue");
        }
    }
}