using System;
using CarsDomain.Properties;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CarsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class ManufacturerSpec
    {
        [TestMethod]
        public void WhenConstructAndZeroYear_ThenThrows()
        {
            Action a = () => new Manufacturer(0, Manufacturer.Makes[0], Manufacturer.Models[0]);
            a.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.Manufacturer_InvalidYear);
        }

        [TestMethod]
        public void WhenConstructAndYearLessThanMin_ThenThrows()
        {
            Action a = () => new Manufacturer(Manufacturer.MinYear - 1, Manufacturer.Makes[0], Manufacturer.Models[0]);
            a.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.Manufacturer_InvalidYear);
        }

        [TestMethod]
        public void WhenConstructAndYearGreaterThanMax_ThenThrows()
        {
            Action a = () => new Manufacturer(Manufacturer.MaxYear + 1, Manufacturer.Makes[0], Manufacturer.Models[0]);
            a.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.Manufacturer_InvalidYear);
        }

        [TestMethod]
        public void WhenConstructAndMakeUnknown_ThenThrows()
        {
            Action a = () => new Manufacturer(Manufacturer.MinYear, "unknown", Manufacturer.Models[0]);
            a.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.Manufacturer_UnknownMake);
        }

        [TestMethod]
        public void WhenConstructAndModelUnknown_ThenThrows()
        {
            Action a = () => new Manufacturer(Manufacturer.MinYear, Manufacturer.Makes[0], "unknown");
            a.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.Manufacturer_UnknownModel);
        }

        [TestMethod]
        public void WhenConstruct_ThenSucceeds()
        {
            var manufacturer = new Manufacturer(Manufacturer.MinYear, Manufacturer.Makes[0], Manufacturer.Models[0]);

            manufacturer.Year.Should().Be(Manufacturer.MinYear);
        }
    }
}