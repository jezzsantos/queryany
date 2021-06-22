using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class TimeSpanExtensionsSpec
    {
        [TestMethod]
        public void WhenToTimeSpanWithNullValue_ThenReturnsZero()
        {
            var result = ((string) null).ToTimeSpan();

            result.Should().Be(TimeSpan.Zero);
        }

        [TestMethod]
        public void WhenToTimeSpanWithNonZeroValue_ThenReturnsValue()
        {
            var result = "PT1H".ToTimeSpan();

            result.Should().Be(TimeSpan.FromHours(1));
        }

        [TestMethod]
        public void WhenToTimeSpanWithOtherValue_ThenReturnsZero()
        {
            var span = TimeSpan.FromDays(1);

            var result = span.ToString().ToTimeSpan();

            result.Should().Be(span);
        }

        [TestMethod]
        public void WhenToTimeSpanWithNullValueAndDefaultValue_ThenReturnsDefaultValue()
        {
            var defaultValue = TimeSpan.FromHours(1);

            var result = ((string) null).ToTimeSpan(defaultValue);

            result.Should().Be(defaultValue);
        }

        [TestMethod]
        public void WhenToTimeSpanWithInvalidTimeSpanValue_ThenReturnsZero()
        {
            var result = "notavalidtimespan".ToTimeSpan();

            result.Should().Be(TimeSpan.Zero);
        }

        [TestMethod]
        public void WhenToTimeSpanWithInvalidTimeSpanValueAndDefaultValue_ThenReturnsDefaultValue()
        {
            var defaultValue = TimeSpan.FromHours(1);

            var result = "notavalidtimespan".ToTimeSpan(defaultValue);

            result.Should().Be(defaultValue);
        }

        [TestMethod]
        public void WhenToTimeSpanWithStringSerializedSpan_ThenReturnsTimeSpan()
        {
            var span = TimeSpan.FromHours(1);

            var result = span.ToString().ToTimeSpan();

            result.Should().Be(span);
        }
    }
}