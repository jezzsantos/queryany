using System;
using System.Linq;
using Api.Interfaces.ServiceOperations;
using CarsApplication.Storage;
using CarsDomain;
using CarsStorage;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceClients;
using ServiceStack;
using Storage.Interfaces;

namespace CarsApi.IntegrationTests
{
    [TestClass, TestCategory("Integration.Web")]
    public class CarsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private static IWebHost webHost;
        private static IStorage<CarEntity> storage;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            webHost = WebHost.CreateDefaultBuilder(null)
                .UseModularStartup<Startup>()
                .UseUrls(ServiceUrl)
                .UseKestrel()
                .ConfigureLogging((ctx, builder) => builder.AddConsole())
                .Build();
            webHost.Start();

            // Override services for testing
            var container = HostContext.Container;
            container.AddSingleton<IPersonsService, StubPersonsService>();
            container.AddSingleton<ICarStorage>(c =>
                new CarStorage(c.Resolve<IStorage<CarEntity>>()));
            storage = CarEntityInMemStorage.Create(container.Resolve<ILogger>(), container.Resolve<IDomainFactory>());
            container.AddSingleton(storage);
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            webHost?.StopAsync().GetAwaiter().GetResult();
        }

        [TestInitialize]
        public void Initialize()
        {
            storage.DestroyAll();
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
            car.Owner.Id.Should().Be(CurrentCallerConstants.AnonymousUserId);
            car.Managers.Single().Id.Should().Be(CurrentCallerConstants.AnonymousUserId);
        }

        [TestMethod]
        public void WhenGetAvailableAndNoCars_ThenReturnsNone()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var cars = client.Get(new SearchAvailableCarsRequest());

            cars.Cars.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenGetAvailableAndCars_ThenReturnsAvailable()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var car = client.Post(new CreateCarRequest
            {
                Year = 2010,
                Make = Manufacturer.Makes[0],
                Model = Manufacturer.Models[0]
            }).Car;
            client.Put(new RegisterCarRequest
            {
                Id = car.Id,
                Jurisdiction = "New Zealand",
                Number = "ABC123"
            });
            client.Put(new OccupyCarRequest
            {
                Id = car.Id,
                UntilUtc = DateTime.UtcNow.Add(TimeSpan.FromMinutes(1))
            });

            var cars = client.Get(new SearchAvailableCarsRequest());

            cars.Cars.Count.Should().Be(1);
            cars.Cars[0].Id.Should().Be(car.Id);
        }
    }
}