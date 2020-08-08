using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;

namespace QueryAny.UnitTests
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
    }
}