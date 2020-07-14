using System;
using CarsDomain.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Interfaces.Apis;
using ServiceStack;
using Storage;
using Storage.Interfaces;

namespace CarsApi.IntegrationTests
{
    [TestClass]
    public class CarsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private ServiceStackHost appHost;
        private CarEntityInMemStorage store;

        [TestInitialize]
        public void Initialize()
        {
            this.store = new CarEntityInMemStorage(new InProcessInMemRepository(new GuidIdentifierFactory()));

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

        [TestMethod, TestCategory("Integration")]
        public void WhenGetAvailableAndNoCars_ThenReturnsNone()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var cars = client.Get(new SearchAvailableCarsRequest());

            cars.Cars.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Integration")]
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