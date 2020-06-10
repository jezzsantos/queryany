using System;
using CarsDomain.Entities;
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
        private CarInMemStorage store;

        [TestInitialize]
        public void Initialize()
        {
            this.store = new CarInMemStorage();

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

            Assert.AreEqual(0, cars.Cars.Count);
        }

        [TestMethod, TestCategory("Integration")]
        public void WhenGetAvailableAndCars_ThenReturnsNone()
        {
            var client = new JsonServiceClient(ServiceUrl);
            this.store.Add(new CarEntity
            {
                Id = "acardid1",
                OccupiedUntilUtc = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1))
            });

            var cars = client.Get(new SearchAvailableCarsRequest());

            Assert.AreEqual(1, cars.Cars.Count);
            Assert.AreEqual("acaridid1", cars.Cars[0].Id);
        }
    }
}