using System;
using System.Linq;
using Api.Interfaces.ServiceOperations;
using CarsDomain;
using CarsStorage;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;
using Storage;
using Storage.Interfaces;

namespace CarsApi.IntegrationTests
{
    [TestClass, TestCategory("Integration")]
    public class CarsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private ServiceStackHost appHost;
        private ILogger logger;
        private CarEntityInMemStorage store;

        [TestInitialize]
        public void Initialize()
        {
            this.appHost = new TestServiceHost();
            this.logger = new Logger<TestServiceHost>(new NullLoggerFactory());
            this.store = CarEntityInMemStorage.Create(this.logger, new GuidIdentifierFactory());
            this.appHost.Container.AddSingleton(this.logger);
            this.appHost.Container.AddSingleton<IStorage<CarEntity>>(this.store);

            this.appHost.Init()
                .Start(ServiceUrl);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.appHost.Dispose();
        }

        [TestMethod]
        public void WhenCreateCar_ThenReturnsCar()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var car = client.Post(new CreateCarRequest
            {
                Year = 2010,
                Make = Manufacturer.Makes[0],
                Model = Manufacturer.Models[0]
            }).Car;

            car.Manufacturer.Year.Should().Be(2010);
            car.Manufacturer.Make.Should().Be(Manufacturer.Makes[0]);
            car.Manufacturer.Model.Should().Be(Manufacturer.Models[0]);
            car.OccupiedUntilUtc.Should().Be(DateTime.MinValue);
            car.Owner.Id.Should().Be("anonymous");
            car.Managers.Single().Id.Should().Be("anonymous");
        }

        [TestMethod]
        public void WhenGetAvailableAndNoCars_ThenReturnsNone()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var cars = client.Get(new SearchAvailableCarsRequest());

            cars.Cars.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenGetAvailableAndCars_ThenReturnsNone()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var car = client.Post(new CreateCarRequest
            {
                Year = 2010,
                Make = Manufacturer.Makes[0],
                Model = Manufacturer.Models[0]
            }).Car;
            client.Put(new OccupyCarRequest
            {
                Id = car.Id,
                UntilUtc = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1))
            });

            var cars = client.Get(new SearchAvailableCarsRequest());

            cars.Cars.Count.Should().Be(1);
            cars.Cars[0].Id.Should().Be(car.Id);
        }
    }
}