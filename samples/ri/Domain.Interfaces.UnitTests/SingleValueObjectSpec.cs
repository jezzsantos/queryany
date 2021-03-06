﻿using FluentAssertions;
using Xunit;

namespace Domain.Interfaces.UnitTests
{
    [Trait("Category", "Unit")]
    public class SingleValueObjectSpec
    {
        private readonly TestSingleStringValueObject valueObject;

        public SingleValueObjectSpec()
        {
            this.valueObject = new TestSingleStringValueObject("avalue");
        }

        [Fact]
        public void WhenAssignInstanceToString_ThenValueAssigned()
        {
            var stringValue = new TestSingleStringValueObject("avalue");

            stringValue.StringValue.Should().Be("avalue");
        }

        [Fact]
        public void WhenAssignInstanceToEnumThenValueAssigned()
        {
            var enumValue = new TestSingleEnumValueObject(TestEnum.AValue1);

            enumValue.EnumValue.Should().Be(TestEnum.AValue1);
        }

        [Fact]
        public void WhenEquateWithSameValueObject_ThenReturnsTrue()
        {
            var result = this.valueObject == new TestSingleStringValueObject("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenEquateWithDifferentValueObject_ThenReturnsFalse()
        {
            var result = this.valueObject == new TestSingleStringValueObject("anothervalue");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEquateWithSameStringValue_ThenReturnsTrue()
        {
            var result = this.valueObject == "avalue";

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenEquateWithDifferentStringValue_ThenReturnsFalse()
        {
            var result = this.valueObject == "anothervalue";

            result.Should().BeFalse();
        }
    }
}