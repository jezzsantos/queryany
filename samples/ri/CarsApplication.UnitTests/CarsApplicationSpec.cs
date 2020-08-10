using System;
using System.Collections.Generic;
using System.Linq;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueryAny;
using Storage.Interfaces;

namespace CarsApplication.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class CarsApplicationSpec
    {
        private Mock<ICurrentCaller> caller;
        private CarsApplication carsApplication;
        private Mock<IIdentifierFactory> idFactory;
        private Mock<ILogger> logger;
        private Mock<IStorage<CarEntity>> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier());
            this.storage = new Mock<IStorage<CarEntity>>();
            this.caller = new Mock<ICurrentCaller>();
            this.caller.Setup(c => c.Id).Returns("acallerid");
            this.carsApplication = new CarsApplication(this.logger.Object, this.idFactory.Object, this.storage.Object);
        }

        [TestMethod]
        public void WhenCreate_ThenReturnsCar()
        {
            var make = Manufacturer.Makes[0];
            var model = Manufacturer.Models[0];
            var entity = new CarEntity(this.logger.Object, this.idFactory.Object);
            this.storage.Setup(s =>
                    s.Add(It.Is<CarEntity>(e =>
                        e.Manufacturer.Year == 2010
                        && e.Manufacturer.Make == make
                        && e.Manufacturer.Model == model
                        && e.Owner.Id == "acallerid"
                        && e.Managers.ManagerIds.Single() == "acallerid"
                        && e.OccupiedUntilUtc == DateTime.MinValue)))
                .Returns(entity);

            var result = this.carsApplication.Create(this.caller.Object, 2010, make, model);

            result.Id.Should().Be("anid");
        }

        [TestMethod]
        public void WhenOccupy_ThenOccupiesAndReturnsCar()
        {
            var untilUtc = DateTime.UtcNow;
            this.storage.Setup(s => s.Get(It.Is<Identifier>(i => i == "acarid")))
                .Returns(new CarEntity(this.logger.Object, this.idFactory.Object));
            this.storage.Setup(s => s.Update(It.Is<CarEntity>(e => e.OccupiedUntilUtc == untilUtc)))
                .Returns(new CarEntity(this.logger.Object, this.idFactory.Object));

            var result = this.carsApplication.Occupy(this.caller.Object, "acarid", untilUtc);

            result.Should().NotBeNull();
        }

        [TestMethod]
        public void WhenSearchAvailable_ThenReturnsAvailableCars()
        {
            this.storage.Setup(s => s.Query(It.IsAny<QueryClause<CarEntity>>()))
                .Returns(new QueryResults<CarEntity>(new List<CarEntity>
                    {new CarEntity(this.logger.Object, this.idFactory.Object)}));

            var result =
                this.carsApplication.SearchAvailable(this.caller.Object, new SearchOptions(), new GetOptions());

            result.Results.Count.Should().Be(1);
        }
    }
}