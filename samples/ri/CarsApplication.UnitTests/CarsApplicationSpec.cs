using System;
using System.Collections.Generic;
using System.Linq;
using CarsApplication.Storage;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceClients;

namespace CarsApplication.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class CarsApplicationSpec
    {
        private Mock<ICurrentCaller> caller;
        private CarsApplication carsApplication;
        private Mock<IIdentifierFactory> idFactory;
        private Mock<ILogger> logger;
        private Mock<IPersonsService> personService;
        private Mock<ICarStorage> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier());
            this.idFactory.Setup(idf => idf.IsValid(It.IsAny<Identifier>()))
                .Returns(true);
            this.storage = new Mock<ICarStorage>();
            this.personService = new Mock<IPersonsService>();
            this.caller = new Mock<ICurrentCaller>();
            this.caller.Setup(c => c.Id).Returns("acallerid");
            this.carsApplication = new CarsApplication(this.logger.Object, this.idFactory.Object, this.storage.Object,
                this.personService.Object);
        }

        [TestMethod]
        public void WhenCreate_ThenReturnsCar()
        {
            var person = new Person
                {Id = "apersonid"};
            this.personService.Setup(ps => ps.Get(It.IsAny<Identifier>()))
                .Returns(person);
            var make = Manufacturer.Makes[0];
            var model = Manufacturer.Models[0];
            var entity = new CarEntity(this.logger.Object, this.idFactory.Object);
            this.storage.Setup(s =>
                    s.Create(It.IsAny<CarEntity>()))
                .Returns(entity);

            var result = this.carsApplication.Create(this.caller.Object, 2010, make, model);

            result.Id.Should().Be("anid");
            this.storage.Verify(s =>
                s.Create(It.Is<CarEntity>(e =>
                    e.Owner == "apersonid"
                    && e.Manufacturer.Year == 2010
                    && e.Manufacturer.Make == make
                    && e.Manufacturer.Model == model
                    && e.Managers.Managers.Single() == "apersonid"
                    && e.OccupiedUntilUtc == DateTime.MinValue)));
        }

        [TestMethod]
        public void WhenOccupy_ThenOccupiesAndReturnsCar()
        {
            var untilUtc = DateTime.UtcNow;
            var entity = new CarEntity(this.logger.Object, this.idFactory.Object);
            entity.SetManufacturer(new Manufacturer(2010, Manufacturer.Makes[0], Manufacturer.Models[0]));
            entity.SetOwnership(new CarOwner {Id = "anownerid"});
            entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));
            this.storage.Setup(s => s.Get(It.Is<Identifier>(i => i == "acarid")))
                .Returns(entity);
            this.storage.Setup(s => s.Update(It.Is<CarEntity>(e => e.OccupiedUntilUtc == untilUtc)))
                .Returns(entity);

            var result = this.carsApplication.Occupy(this.caller.Object, "acarid", untilUtc);

            result.OccupiedUntilUtc.Should().Be(untilUtc);
        }

        [TestMethod]
        public void WhenSearchAvailable_ThenReturnsAvailableCars()
        {
            this.storage.Setup(s => s.SearchAvailable(It.IsAny<SearchOptions>()))
                .Returns(new List<CarEntity>
                    {new CarEntity(this.logger.Object, this.idFactory.Object)});

            var result =
                this.carsApplication.SearchAvailable(this.caller.Object, new SearchOptions(), new GetOptions());

            result.Results.Count.Should().Be(1);
        }
    }
}