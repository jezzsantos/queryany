using System;
using System.Linq;
using CarsDomain.Properties;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CarsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class CarEntitySpec
    {
        private CarEntity entity;
        private Mock<IIdentifierFactory> identifierFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.identifierFactory.Setup(f => f.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier);
            this.entity = new CarEntity(this.logger.Object, this.identifierFactory.Object);
        }

        [TestMethod]
        public void WhenSetManufacturer_ThenManufactured()
        {
            var manufacturer =
                new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0], Manufacturer.Models[0]);
            this.entity.SetManufacturer(manufacturer);

            this.entity.Manufacturer.Should()
                .Be(manufacturer);
            this.entity.Events[1].Should().BeOfType<Events.Car.ManufacturerChanged>();
        }

        [TestMethod]
        public void WhenSetOwnership_ThenOwnedAndManaged()
        {
            var owner = new CarOwner {Id = "anownerid"};
            this.entity.SetOwnership(owner);

            this.entity.Owner.Should().Be(new VehicleOwner(owner.Id));
            this.entity.Managers.Managers.Single().Should().Be("anownerid".ToIdentifier());
            this.entity.Events[1].Should().BeOfType<Events.Car.OwnershipChanged>();
        }

        [TestMethod]
        public void WhenRegistered_ThenRegistered()
        {
            this.entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));

            this.entity.Plate.Should().Be(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));
            this.entity.Events[1].Should().BeOfType<Events.Car.RegistrationChanged>();
        }

        [TestMethod]
        public void WhenOccupyAndNotManufactured_ThenThrows()
        {
            this.entity.SetOwnership(new CarOwner {Id = "anownerid"});
            this.entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));

            this.entity.Invoking(x => x.Occupy(DateTime.UtcNow))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.CarEntity_NotManufactured);
        }

        [TestMethod]
        public void WhenOccupyAndNotOwned_ThenThrows()
        {
            this.entity.SetManufacturer(new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0],
                Manufacturer.Models[0]));
            this.entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));

            this.entity.Invoking(x => x.Occupy(DateTime.UtcNow))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.CarEntity_NotOwned);
        }

        [TestMethod]
        public void WhenOccupyAndNotRegistered_ThenThrows()
        {
            this.entity.SetManufacturer(new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0],
                Manufacturer.Models[0]));
            this.entity.SetOwnership(new CarOwner {Id = "anownerid"});

            this.entity.Invoking(x => x.Occupy(DateTime.UtcNow))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.CarEntity_NotRegistered);
        }

        [TestMethod]
        public void WhenOccupy_ThenOccupied()
        {
            var datum = DateTime.UtcNow.AddDays(1);
            this.entity.SetManufacturer(new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0],
                Manufacturer.Models[0]));
            this.entity.SetOwnership(new CarOwner {Id = "anownerid"});
            this.entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));
            this.entity.Occupy(datum);

            this.entity.OccupiedUntilUtc.Should().Be(datum);
            this.entity.Events[4].Should().BeOfType<Events.Car.OccupancyChanged>();
        }
    }
}