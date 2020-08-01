using System;
using Api.Interfaces.ServiceOperations;
using CarsDomain.Entities;
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
    [TestClass, TestCategory("Integration.NOCI")]
    public class CarsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private ServiceStackHost appHost;
        private ILogger logger;
        private CarEntityInMemStorage store;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Logger<CarsApiSpec>(new NullLoggerFactory());
            this.store = CarEntityInMemStorage.Create(this.logger, new GuidIdentifierFactory());

            this.appHost = new TestAppHost();
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
                Make = "Honda",
                Model = "Civic"
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