using System;
using CarsDomain.Properties;
using FluentAssertions;
using UnitTesting.Common;
using Xunit;

namespace CarsDomain.UnitTests
{
    [Trait("Category", "Unit")]
    public class TimeSlotSpec
    {
        [Fact]
        public void WhenConstructAndMinFrom_ThenThrows()
        {
            Action a = () => new TimeSlot(DateTime.MinValue, DateTime.UtcNow);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void WhenConstructAndMinTo_ThenThrows()
        {
            Action a = () => new TimeSlot(DateTime.UtcNow, DateTime.MinValue);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void WhenConstructAndToNotAfterFrom_ThenThrows()
        {
            var datum = DateTime.UtcNow;
            Action a = () => new TimeSlot(datum, datum);
            a.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.TimeSlot_FromDateBeforeToDate);
        }

        [Fact]
        public void WhenConstructAndToAfterFrom_ThenSucceeds()
        {
            var datum = DateTime.UtcNow;
            var slot = new TimeSlot(datum, datum.AddMilliseconds(1));

            slot.From.Should().Be(datum);
            slot.To.Should().BeAfter(datum);
        }
    }
}