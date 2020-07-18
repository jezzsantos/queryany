using System;
using CarsDomain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services.Interfaces;
using Services.Interfaces.Entities;
using Storage.Interfaces;

namespace CarsDomain.UnitTests
{
    [TestClass]
    public class CarsSpec
    {
        private Mock<ICurrentCaller> caller;
        private Cars cars;
        private Mock<ILogger<Cars>> logger;
        private Mock<IStorage<CarEntity>> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger<Cars>>();
            this.storage = new Mock<IStorage<CarEntity>>();
            this.caller = new Mock<ICurrentCaller>();
            this.cars = new Cars(this.logger.Object, this.storage.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreate_ThenReturnsCar()
        {
            this.storage.Setup(s =>
                    s.Add(It.Is<CarEntity>(e =>
                        e.Model.Year == 2010 && e.Model.Make == "amake" && e.Model.Model == "amodel")))
                .Returns(Identifier.Create("acarid"));

            var result = this.cars.Create(this.caller.Object, 2010, "amake", "amodel");

            result.Id.Should().Be("acarid");
            result.Model.Year.Should().Be(2010);
            result.Model.Make.Should().Be("amake");
            result.Model.Model.Should().Be("amodel");
            result.OccupiedUntilUtc.Should().Be(DateTime.MinValue);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOccupy_ThenOccupiesAndReturnsCar()
        {
            var untilUtc = DateTime.UtcNow;
            var entity = new CarEntity(this.logger.Object);
            this.storage.Setup(s => s.Get(It.Is<Identifier>(i => i.Get() == "acarid")))
                .Returns(entity);
            this.storage.Setup(s => s.Update(It.Is<CarEntity>(e => e.OccupiedUntilUtc == untilUtc)));

            var result = this.cars.Occupy(this.caller.Object, "acarid", untilUtc);

            result.Should().NotBeNull();
        }
    }
}