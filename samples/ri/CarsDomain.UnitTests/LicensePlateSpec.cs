using System;
using CarsDomain.Properties;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CarsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class LicensePlateSpec
    {
        [TestMethod]
        public void WhenConstructAndNullJurisdiction_ThenThrows()
        {
            Action a = () => new LicensePlate(null, "anumber");
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenConstructAndUnknownJurisdiction_ThenThrows()
        {
            Action a = () => new LicensePlate("unknown", "anumber");
            a.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.LicensePlate_UnknownJurisdiction);
        }

        [TestMethod]
        public void WhenConstructAndNullNumber_ThenThrows()
        {
            Action a = () => new LicensePlate(LicensePlate.Jurisdictions[0], null);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenConstructAndInvalidNumber_ThenThrows()
        {
            Action a = () => new LicensePlate(LicensePlate.Jurisdictions[0], "^invalid^");
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void WhenConstructAndKnownJurisdiction_ThenSucceeds()
        {
            var plate = new LicensePlate(LicensePlate.Jurisdictions[0], "anumber");

            plate.Jurisdiction.Should().Be(LicensePlate.Jurisdictions[0]);
            plate.Number.Should().Be("anumber");
        }
    }
}