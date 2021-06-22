using System;
using System.Linq;
using Api.Interfaces.ServiceOperations.Cars;
using Application.Storage.Interfaces;
using CarsApplication.ReadModels;
using CarsDomain;
using Domain.Interfaces;
using FluentAssertions;
using IntegrationTesting.Common;
using Xunit;
using Car = Application.Common.Resources.Car;

namespace CarsApi.IntegrationTests
{
    [Trait("Category", "Integration.Web"), Collection("ThisAssembly")]
    public class CarsApiSpec : IClassFixture<ApiSpecSetup<TestStartup>>
    {
        private static int plateCount;
        private readonly ApiSpecSetup<TestStartup> setup;

        public CarsApiSpec(ApiSpecSetup<TestStartup> setup)
        {
            this.setup = setup;
            this.setup.Resolve<IQueryStorage<CarsApplication.ReadModels.Car>>().DestroyAll();
            this.setup.Resolve<IEventStreamStorage<CarEntity>>().DestroyAll();
            this.setup.Resolve<IQueryStorage<Unavailability>>().DestroyAll();
        }

        [Fact]
        public void WhenCreateCar_ThenReturnsCar()
        {
            var car = this.setup.Api.Post(new CreateCarRequest
            {
                Year = 2010,
                Make = Manufacturer.Makes[0],
                Model = Manufacturer.Models[0]
            }).Car;

            car.Manufacturer.Year.Should().Be(2010);
            car.Manufacturer.Make.Should().Be(Manufacturer.Makes[0]);
            car.Manufacturer.Model.Should().Be(Manufacturer.Models[0]);
            car.Owner.Id.Should().Be(CallerConstants.AnonymousUserId);
            car.Managers.Single().Id.Should().Be(CallerConstants.AnonymousUserId);
        }

        [Fact]
        public void WhenGetAvailableAndNoCars_ThenReturnsNone()
        {
            var cars = this.setup.Api.Get(new SearchAvailableCarsRequest());

            cars.Cars.Count.Should().Be(0);
        }

        [Fact]
        public void WhenGetAvailableAndCars_ThenReturnsAvailable()
        {
            var car1 = RegisterCar();
            var car2 = RegisterCar();

            var datum = DateTime.UtcNow.AddDays(1);
            this.setup.Api.Put(new OfflineCarRequest
            {
                Id = car1.Id,
                FromUtc = datum,
                ToUtc = datum.AddDays(1)
            });

            var cars = this.setup.Api.Get(new SearchAvailableCarsRequest
            {
                FromUtc = datum,
                ToUtc = datum.AddDays(1)
            });

            cars.Cars.Count.Should().Be(1);
            cars.Cars[0].Id.Should().Be(car2.Id);
        }

        private Car RegisterCar()
        {
            var car1 = this.setup.Api.Post(new CreateCarRequest
            {
                Year = 2010,
                Make = Manufacturer.Makes[0],
                Model = Manufacturer.Models[0]
            }).Car;
            this.setup.Api.Put(new RegisterCarRequest
            {
                Id = car1.Id,
                Jurisdiction = "New Zealand",
                Number = $"ABC{++plateCount:###}"
            });
            return car1;
        }
    }
}