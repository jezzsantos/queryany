using System;
using FluentAssertions;
using QueryAny.Extensions;
using Xunit;

namespace QueryAny.UnitTests.Extensions
{
    [Trait("Category", "Unit")]
    public class DateTimeExtensionsSpec
    {
        [Fact]
        public void WhenHasValueAndMinValue_ThenReturnsFalse()
        {
            var result = DateTime.MinValue.HasValue();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasValueAndMinValueToUtc_ThenReturnsFalse()
        {
            var result = DateTime.MinValue.ToUniversalTime().HasValue();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasValueAndMinValueToLocal_ThenReturnsFalse()
        {
            var result = DateTime.MinValue.ToLocalTime().HasValue();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasValueAndUtcMinValue_ThenReturnsFalse()
        {
            var result = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc).HasValue();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasValueAndLocalMinValue_ThenReturnsFalse()
        {
            var result = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Local).HasValue();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasValueAndUnspecifiedMinValue_ThenReturnsFalse()
        {
            var result = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Unspecified).HasValue();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasValueAndNotMinValue_ThenReturnsTrue()
        {
            var result = DateTime.UtcNow.HasValue();

            result.Should().BeTrue();
        }
    }
}