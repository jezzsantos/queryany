using System;
using Domain.Interfaces.Properties;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class DateTimeExtensionsSpec
    {
        [TestMethod]
        public void WhenHasValueAndMinValue_ThenReturnsFalse()
        {
            var result = DateTime.MinValue.HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndMinValueToUtc_ThenReturnsFalse()
        {
            var result = DateTime.MinValue.ToUniversalTime().HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndMinValueToLocal_ThenReturnsFalse()
        {
            var result = DateTime.MinValue.ToLocalTime().HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndUtcMinValue_ThenReturnsFalse()
        {
            var result = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc).HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndLocalMinValue_ThenReturnsFalse()
        {
            var result = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Local).HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndUnspecifiedMinValue_ThenReturnsFalse()
        {
            var result = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Unspecified).HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndNotMinValue_ThenReturnsTrue()
        {
            var result = DateTime.UtcNow.HasValue();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenToLocalTimeWithMinDateTime_ThenThrows()
        {
            DateTime.MinValue
                .Invoking(x => x.ToLocalTime(Timezones.Default))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void WhenToLocalTimeWithLocalDateTime_ThenThrows()
        {
            DateTime.Now
                .Invoking(x => x.ToLocalTime(Timezones.Default))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.DateTimeExtensions_DateTimeIsNotUtc);
        }

        [TestMethod]
        public void WhenToLocalTimeWithNullTimezone_ThenThrows()
        {
            DateTime.UtcNow
                .Invoking(x => x.ToLocalTime(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenToLocalTimeWithInvalidTimezone_ThenThrows()
        {
            DateTime.UtcNow
                .Invoking(x => x.ToLocalTime("aninvalidianatimezone"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.DateTimeExtensions_InvalidTimezone);
        }

        [TestMethod]
        public void WhenToLocalTimeWithLocalTimezoneOfThisMachine_ThenReturnsLocalTime()
        {
            var datum = DateTime.Now.ToLocalTime();
            var timezoneInfo = TimeZoneInfo.Local.ToIana();

            var result = datum.ToUniversalTime().ToLocalTime(timezoneInfo);

            result.Should().Be(datum);
        }
    }
}