using System;
using System.Linq;
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
        public void WhenOccupy_ThenOccupied()
        {
            var datum = DateTime.UtcNow.AddDays(1);
            this.entity.Occupy(datum);

            this.entity.OccupiedUntilUtc.Should().Be(datum);
            this.entity.Events[1].Should().BeOfType<Events.Car.OccupancyChanged>();
        }

        [TestMethod]
        public void WhenSetManufacturer_ThenManufactured()
        {
            this.entity.SetManufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0], Manufacturer.Models[0]);

            this.entity.Manufacturer.Should()
                .Be(new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0], Manufacturer.Models[0]));
            this.entity.Events[1].Should().BeOfType<Events.Car.ManufacturerChanged>();
        }

        [TestMethod]
        public void WhenSetOwnership_ThenOwnedAndManaged()
        {
            var owner = new CarOwner {Id = "anownerid"};
            this.entity.SetOwnership(owner);

            this.entity.Owner.Should().Be(new VehicleOwner(owner));
            this.entity.Managers.Managers.Single().Should().Be("anownerid".ToIdentifier());
            this.entity.Events[1].Should().BeOfType<Events.Car.OwnershipChanged>();
        }

        [TestMethod]
        public void WhenRegistered_ThenRegistered()
        {
            this.entity.Register("ajurisdiction", "anumber");

            this.entity.Plate.Should().Be(new LicensePlate("ajurisdiction", "anumber"));
            this.entity.Events[1].Should().BeOfType<Events.Car.RegistrationChanged>();
        }
    }
}