using System;
using Common.Properties;
using FluentAssertions;
using UnitTesting.Common;
using Xunit;

namespace Common.UnitTests
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

        [Fact]
        public void WhenToLocalTimeWithMinDateTime_ThenThrows()
        {
            DateTime.MinValue
                .Invoking(x => x.ToLocalTime(Timezones.Default))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void WhenToLocalTimeWithLocalDateTime_ThenThrows()
        {
            DateTime.Now
                .Invoking(x => x.ToLocalTime(Timezones.Default))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.DateTimeExtensions_DateTimeIsNotUtc);
        }

        [Fact]
        public void WhenToLocalTimeWithNullTimezone_ThenThrows()
        {
            DateTime.UtcNow
                .Invoking(x => x.ToLocalTime(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenToLocalTimeWithInvalidTimezone_ThenThrows()
        {
            DateTime.UtcNow
                .Invoking(x => x.ToLocalTime("aninvalidianatimezone"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.DateTimeExtensions_InvalidTimezone);
        }

        [Fact]
        public void WhenToLocalTimeWithLocalTimezoneOfThisMachine_ThenReturnsLocalTime()
        {
            var datum = DateTime.Now.ToLocalTime();
            var timezoneInfo = TimeZoneInfo.Local.ToIana();

            var result = datum.ToUniversalTime().ToLocalTime(timezoneInfo);

            result.Should().Be(datum);
        }
    }
}