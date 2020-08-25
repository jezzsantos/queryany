using System;
using CarsDomain.Properties;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CarsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class TimeSlotSpec
    {
        [TestMethod]
        public void WhenConstructAndMinFrom_ThenThrows()
        {
            Action a = () => new TimeSlot(DateTime.MinValue, DateTime.UtcNow);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void WhenConstructAndMinTo_ThenThrows()
        {
            Action a = () => new TimeSlot(DateTime.UtcNow, DateTime.MinValue);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void WhenConstructAndToNotAfterFrom_ThenThrows()
        {
            var datum = DateTime.UtcNow;
            Action a = () => new TimeSlot(datum, datum);
            a.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.TimeSlot_FromDateBeforeToDate);
        }

        [TestMethod]
        public void WhenConstructAndToAfterFrom_ThenSucceeds()
        {
            var datum = DateTime.UtcNow;
            var slot = new TimeSlot(datum, datum.AddMilliseconds(1));

            slot.From.Should().Be(datum);
            slot.To.Should().BeAfter(datum);
        }
    }
}