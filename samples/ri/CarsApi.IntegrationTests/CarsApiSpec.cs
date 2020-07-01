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
            this.store = new CarInMemStorage(new InProcessInMemRepository(new GuidIdentifierFactory()));

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
            var car = new CarEntity();
            car.Occupy(DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1)));
            var carId = this.store.Add(car);

            var cars = client.Get(new SearchAvailableCarsRequest());

            Assert.AreEqual(1, cars.Cars.Count);
            Assert.AreEqual(carId, cars.Cars[0].Id);
        }
    }
}