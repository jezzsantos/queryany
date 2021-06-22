using Domain.Interfaces.Entities;
using FluentAssertions;
using ServiceStack;
using Xunit;

namespace Domain.Interfaces.UnitTests
{
    [Trait("Category", "Unit")]
    public class IdentifierSpec
    {
        [Fact]
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