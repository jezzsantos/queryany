using System;
using System.Linq.Expressions;
using FluentAssertions;
using QueryAny.Extensions;
using QueryAny.Properties;
using Xunit;

namespace QueryAny.UnitTests.Extensions
{
    [Trait("Category", "Unit")]
    public class ReflectorSpec
    {
        [Fact]
        public void WhenGetPropertyNameForSimpleProperty_ThenReturnsName()
        {
            Expression<Func<TestReflectionClass, string>> propertyName = @class => @class.AStringValue;

            var result = Reflector<TestReflectionClass>.GetPropertyName(propertyName);

            result.Should().Be(nameof(TestReflectionClass.AStringValue));
        }

        [Fact]
        public void WhenGetPropertyNameForOptionalProperty_ThenReturnsName()
        {
            var result = Reflector<TestReflectionClass>.GetPropertyName(@class => @class.AClassOfStringValue);

            result.Should().Be(nameof(TestReflectionClass.AClassOfStringValue));
        }

        [Fact]
        public void WhenGetPropertyNameForAMethod_ThenThrows()
        {
            Expression<Func<TestReflectionClass, string>> propertyName = @class => @class.AMethod();

            FluentActions.Invoking(() => Reflector<TestReflectionClass>.GetPropertyName(propertyName))
                .Should().Throw<ArgumentException>()
                .WithMessageLike(Resources.Reflector_ErrorNotMemberAccessOrConvertible);
        }
    }

    internal class TestReflectionClass
    {
        public TestClass<string> AClassOfStringValue { get; } = null!;

        public string AStringValue { get; } = null!;

        public string AMethod()
        {
            return string.Empty;
        }
    }

    // ReSharper disable once UnusedTypeParameter
    internal class TestClass<TValue>
    {
    }
}