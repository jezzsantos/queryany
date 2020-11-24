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
using InfrastructureServices.Eventing.ReadModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;
using Storage;
using Storage.Interfaces;
using Storage.ReadModels;
using IRepository = Storage.IRepository;

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
        private static IRepository repository;

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
            repository = new InProcessInMemRepository();

            carQueryStorage = new GeneralQueryStorage<Car>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), repository);
            carEventingStorage = new GeneralEventStreamStorage<CarEntity>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(),
                container.Resolve<IChangeEventMigrator>(), repository);
            unavailabilityQueryStorage = new GeneralQueryStorage<Unavailability>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), repository);

            container.AddSingleton(carEventingStorage);
            container.AddSingleton<ICarStorage>(c =>
                new CarStorage(carQueryStorage, carEventingStorage, unavailabilityQueryStorage));
            container.AddSingleton<IReadModelProjectionSubscription>(c => new InProcessReadModelProjectionSubscription(
                c.Resolve<ILogger>(),
                new ReadModelProjector(c.Resolve<ILogger>(),
                    new ReadModelCheckpointStore(c.Resolve<ILogger>(), c.Resolve<IIdentifierFactory>(),
                        c.Resolve<IDomainFactory>(), repository),
                    c.Resolve<IChangeEventMigrator>(),
                    new CarEntityReadModelProjection(c.Resolve<ILogger>(), repository)),
                c.Resolve<IEventStreamStorage<CarEntity>>()));

            //HACK: subscribe again (see: https://forums.servicestack.net/t/integration-testing-and-overriding-registered-services/8875/5)
            HostContext.AppHost.OnAfterInit();
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