using System;
using System.Linq;
using Api.Interfaces.ServiceOperations;
using ApplicationServices;
using CarsApplication.ReadModels;
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
using ServiceStack;
using Storage;
using Storage.Interfaces;
using IRepository = Storage.IRepository;

namespace CarsApi.IntegrationTests
{
    [TestClass, TestCategory("Integration.Web")]
    public class CarsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private static IWebHost webHost;
        private static ICommandStorage<CarEntity> carCommandStorage;
        private static IQueryStorage<Car> carQueryStorage;
        private static IEventingStorage<CarEntity> carEventingStorage;
        private static ICommandStorage<UnavailabilityEntity> unavailabilityCommandStorage;
        private static IQueryStorage<Unavailability> unavailabilityQueryStorage;
        private static int plateCount;
        private static IRepository inMemRepository;

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
            inMemRepository = new InProcessInMemRepository();

            carCommandStorage =
                new GeneralCommandStorage<CarEntity>(container.Resolve<ILogger>(), container.Resolve<IDomainFactory>(),
                    inMemRepository);
            carQueryStorage = new GeneralQueryStorage<Car>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), inMemRepository);
            carEventingStorage = new GeneralEventingStorage<CarEntity>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), inMemRepository);
            unavailabilityCommandStorage = new GeneralCommandStorage<UnavailabilityEntity>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(),
                inMemRepository);
            unavailabilityQueryStorage = new GeneralQueryStorage<Unavailability>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), inMemRepository);
            container.AddSingleton<ICarStorage>(c =>
                new CarStorage(carQueryStorage, carEventingStorage, unavailabilityQueryStorage));
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            webHost?.StopAsync().GetAwaiter().GetResult();
        }

        [TestInitialize]
        public void Initialize()
        {
            carCommandStorage.DestroyAll();
            carQueryStorage.DestroyAll();
            carEventingStorage.DestroyAll();
            unavailabilityCommandStorage.DestroyAll();
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

        private static Domain.Interfaces.Resources.Car RegisterCar(IRestClient client)
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