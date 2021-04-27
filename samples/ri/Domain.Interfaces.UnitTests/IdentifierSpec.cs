using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class IdentifierSpec
    {
        [TestMethod]
        public void WhenAutoMapperMapsIdentifier_ThenMapsToStringValue()
        {
            var @object = new TestObject
            {
                StringValue = Identifier.Create("avalue")
            };

            var result = @object.ConvertTo<TestDto>();

            result.StringValue.Should().Be("avalue");
        }
    }

    public class TestObject
    {
        public Identifier StringValue { get; set; }
    }

    public class TestDto
    {
        public string StringValue { get; set; }
    }
}