using System;
using CarsApi.Storage;
using CarsDomain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Interfaces.Apis;
using ServiceStack;
using Storage.Interfaces;

namespace CarsApi.IntegrationTests
{
    [TestClass]
    public class CarsSpec
    {
        private const string serviceUrl = "http://localhost:2000/";
        private ServiceStackHost appHost;
        private CarInMemStorage store;

        [TestInitialize]
        public void Initialize()
        {
            this.store = new CarInMemStorage();

            this.appHost = new TestAppHost();
            this.appHost.Container.AddSingleton<IStorage<CarEntity>>(this.store);

            this.appHost.Init()
            .Start(serviceUrl);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.appHost.Dispose();
        }

        [TestMethod]
        public void WhenGetAvailableAndNoCars_ThenReturnsNone()
        {
            var client = new JsonServiceClient(serviceUrl);

            var cars = client.Get(new GetAvailableCars());

            Assert.AreEqual(0, cars.Cars.Count);
        }

        [TestMethod]
        public void WhenGetAvailableAndCars_ThenReturnsNone()
        {
            var client = new JsonServiceClient(serviceUrl);
            this.store.Add(new CarEntity
            {
                Id = "acardid1",
                OccupiedUntilUtc = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1))
            });

            var cars = client.Get(new GetAvailableCars());

            Assert.AreEqual(1, cars.Cars.Count);
            Assert.AreEqual("acaridid1", cars.Cars[0].Id);
        }
    }
}
