using System;
using FluentAssertions;
using Xunit;

namespace Common.UnitTests
{
    [Trait("Category", "Unit")]
    public class TimeSpanExtensionsSpec
    {
        [Fact]
        public void WhenToTimeSpanWithNullValue_ThenReturnsZero()
        {
            var result = ((string) null).ToTimeSpan();

            result.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void WhenToTimeSpanWithNonZeroValue_ThenReturnsValue()
        {
            var result = "PT1H".ToTimeSpan();

            result.Should().Be(TimeSpan.FromHours(1));
        }

        [Fact]
        public void WhenToTimeSpanWithOtherValue_ThenReturnsZero()
        {
            var span = TimeSpan.FromDays(1);

            var result = span.ToString().ToTimeSpan();

            result.Should().Be(span);
        }

        [Fact]
        public void WhenToTimeSpanWithNullValueAndDefaultValue_ThenReturnsDefaultValue()
        {
            var defaultValue = TimeSpan.FromHours(1);

            var result = ((string) null).ToTimeSpan(defaultValue);

            result.Should().Be(defaultValue);
        }

        [Fact]
        public void WhenToTimeSpanWithInvalidTimeSpanValue_ThenReturnsZero()
        {
            var result = "notavalidtimespan".ToTimeSpan();

            result.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void WhenToTimeSpanWithInvalidTimeSpanValueAndDefaultValue_ThenReturnsDefaultValue()
        {
            var defaultValue = TimeSpan.FromHours(1);

            var result = "notavalidtimespan".ToTimeSpan(defaultValue);

            result.Should().Be(defaultValue);
        }

        [Fact]
        public void WhenToTimeSpanWithStringSerializedSpan_ThenReturnsTimeSpan()
        {
            var span = TimeSpan.FromHours(1);

            var result = span.ToString().ToTimeSpan();

            result.Should().Be(span);
        }
    }
}