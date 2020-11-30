using System;
using System.Linq;
using Api.Interfaces.ServiceOperations.Cars;
using CarsApplication.ReadModels;
using CarsDomain;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;
using Storage.Interfaces;

namespace CarsApi.IntegrationTests
{
    [TestClass, TestCategory("Integration.Web")]
    public class CarsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private static IWebHost webHost;
        private static IQueryStorage<Car> carQueryStorage;
        private static IEventStreamStorage<CarEntity> carEventingStorage;
        private static IQueryStorage<Unavailability> unavailabilityQueryStorage;
        private static int plateCount;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            webHost = WebHost.CreateDefaultBuilder(null)
                .UseModularStartup<TestStartup>()
                .UseUrls(ServiceUrl)
                .UseKestrel()
                .ConfigureLogging((ctx, builder) => builder.AddConsole())
                .Build();
            webHost.Start();

            var container = HostContext.Container;
            carQueryStorage = container.Resolve<IQueryStorage<Car>>();
            carEventingStorage = container.Resolve<IEventStreamStorage<CarEntity>>();
            unavailabilityQueryStorage = container.Resolve<IQueryStorage<Unavailability>>();
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            webHost?.StopAsync().GetAwaiter().GetResult();
        }

        [TestInitialize]
        public void Initialize()
        {
            carQueryStorage.DestroyAll();
            carEventingStorage.DestroyAll();
            unavailabilityQueryStorage.DestroyAll();
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

            var car1 = RegisterCar(client);
            var car2 = RegisterCar(client);

            var datum = DateTime.UtcNow.AddDays(1);
            client.Put(new OfflineCarRequest
            {
                Id = car1.Id,
                FromUtc = datum,
                ToUtc = datum.AddDays(1)
            });

            var cars = client.Get(new SearchAvailableCarsRequest
            {
                FromUtc = datum,
                ToUtc = datum.AddDays(1)
            });

            cars.Cars.Count.Should().Be(1);
            cars.Cars[0].Id.Should().Be(car2.Id);
        }

        private static Application.Resources.Car RegisterCar(IRestClient client)
        {
            var car1 = client.Post(new CreateCarRequest
            {
                Year = 2010,
                Make = Manufacturer.Makes[0],
                Model = Manufacturer.Models[0]
            }).Car;
            client.Put(new RegisterCarRequest
            {
                Id = car1.Id,
                Jurisdiction = "New Zealand",
                Number = $"ABC{++plateCount:###}"
            });
            return car1;
        }
    }
}