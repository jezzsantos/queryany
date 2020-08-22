using System;
using System.Collections.Generic;
using System.Linq;
using CarsApplication.Storage;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using ServiceClients;
using ServiceStack;

namespace CarsApplication
{
    public class CarsApplication : ApplicationBase, ICarsApplication
    {
        private readonly IIdentifierFactory idFactory;
        private readonly ILogger logger;
        private readonly IPersonsService personsService;
        private readonly ICarStorage storage;

        public CarsApplication(ILogger logger, IIdentifierFactory idFactory, ICarStorage storage,
            IPersonsService personsService)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            storage.GuardAgainstNull(nameof(storage));
            personsService.GuardAgainstNull(nameof(personsService));
            this.logger = logger;
            this.idFactory = idFactory;
            this.storage = storage;
            this.personsService = personsService;
        }

        public Car Create(ICurrentCaller caller, int year, string make, string model)
        {
            caller.GuardAgainstNull(nameof(caller));

            var owner = this.personsService.Get(caller.Id.ToIdentifier())
                .ToOwner();

            var car = new CarEntity(this.logger, this.idFactory);
            car.SetOwnership(owner);
            car.SetManufacturer(new Manufacturer(year, make, model));

            var created = this.storage.Create(car);

            this.logger.LogInformation("Car {Id} was created by {Caller}", created.Id, caller.Id);

            return created.ToCar();
        }

        public Car Occupy(ICurrentCaller caller, string id, DateTime untilUtc)
        {
            caller.GuardAgainstNull(nameof(caller));
            id.GuardAgainstNullOrEmpty(nameof(id));

            var car = this.storage.Get(id.ToIdentifier());
            if (id == null)
            {
                throw new ResourceNotFoundException();
            }

            car.Occupy(untilUtc);
            var updated = this.storage.Update(car);

            this.logger.LogInformation("Car {Id} was occupied until {Until}, by {Caller}", id, untilUtc, caller.Id);

            return updated.ToCar();
        }

        public SearchResults<Car> SearchAvailable(ICurrentCaller caller, SearchOptions searchOptions,
            GetOptions getOptions)
        {
            caller.GuardAgainstNull(nameof(caller));

            var cars = this.storage.SearchAvailable(searchOptions);

            this.logger.LogInformation("Available carsApplication were retrieved by {Caller}", caller.Id);

            return searchOptions.ApplyWithMetadata(cars
                .ConvertAll(c => WithGetOptions(c.ToCar(), getOptions)));
        }

        // ReSharper disable once UnusedParameter.Local
        private static Car WithGetOptions(Car car, GetOptions options)
        {
            // TODO: expand embedded resources, etc
            return car;
        }
    }

    public static class CarConversionExtensions
    {
        public static Car ToCar(this CarEntity entity)
        {
            var dto = entity.ConvertTo<Car>();
            dto.Id = entity.Id;
            dto.Owner = entity.Owner.ToOwner();
            dto.Managers = entity.Managers.ToManagers();

            return dto;
        }

        private static List<CarManager> ToManagers(this VehicleManagers managers)
        {
            return managers.HasValue()
                ? new List<CarManager>(managers.Managers.Select(id => new CarManager {Id = id}))
                : new List<CarManager>();
        }

        private static CarOwner ToOwner(this VehicleOwner owner)
        {
            return owner.HasValue()
                ? new CarOwner {Id = owner}
                : null;
        }

        public static CarOwner ToOwner(this Person person)
        {
            var owner = person.ConvertTo<CarOwner>();

            return owner;
        }
    }
}