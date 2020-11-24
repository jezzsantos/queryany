using System;
using System.Collections.Generic;
using System.Linq;
using Application;
using Application.Resources;
using ApplicationServices;
using CarsApplication.Storage;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
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

            var owner = this.personsService.Get(caller.Id)
                .ToOwner();

            var car = new CarEntity(this.logger, this.idFactory);
            car.SetOwnership(new VehicleOwner(owner.Id));
            car.SetManufacturer(new Manufacturer(year, make, model));

            var created = this.storage.Save(car);

            this.logger.LogInformation("Car {Id} was created by {Caller}", created.Id, caller.Id);

            return created.ToCar();
        }

        public Car Offline(ICurrentCaller caller, string id, DateTime fromUtc, DateTime toUtc)
        {
            caller.GuardAgainstNull(nameof(caller));
            id.GuardAgainstNullOrEmpty(nameof(id));
            fromUtc.GuardAgainstMinValue(nameof(fromUtc));
            toUtc.GuardAgainstMinValue(nameof(toUtc));

            var car = this.storage.Load(id.ToIdentifier());
            if (id == null)
            {
                throw new ResourceNotFoundException();
            }

            car.Offline(new TimeSlot(fromUtc, toUtc));
            var updated = this.storage.Save(car);

            this.logger.LogInformation("Car {Id} was taken offline from {From} until {To}, by {Caller}",
                id, fromUtc, toUtc, caller.Id);

            return updated.ToCar();
        }

        public Car Register(ICurrentCaller caller, string id, string jurisdiction, string number)
        {
            caller.GuardAgainstNull(nameof(caller));
            id.GuardAgainstNullOrEmpty(nameof(id));

            var car = this.storage.Load(id.ToIdentifier());
            if (id == null)
            {
                throw new ResourceNotFoundException();
            }

            var plate = new LicensePlate(jurisdiction, number);
            car.Register(plate);
            var updated = this.storage.Save(car);

            this.logger.LogInformation("Car {Id} was registered with plate {Plate}, by {Caller}", id, plate, caller.Id);

            return updated.ToCar();
        }

        public SearchResults<Car> SearchAvailable(ICurrentCaller caller, DateTime fromUtc, DateTime toUtc,
            SearchOptions searchOptions,
            GetOptions getOptions)
        {
            caller.GuardAgainstNull(nameof(caller));

            var cars = this.storage.SearchAvailable(fromUtc, toUtc, searchOptions);

            this.logger.LogInformation("Available carsApplication were retrieved by {Caller}", caller.Id);

            return searchOptions.ApplyWithMetadata(cars
                .ConvertAll(c => WithGetOptions(c.ToCar(), getOptions)));
        }

        public void UpdateManagerEmail(ICurrentCaller caller, string managerId, string email)
        {
            caller.GuardAgainstNull(nameof(caller));

            //TODO: find the cars that have this manager, and update them, then raise email notification
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
        public static Car ToCar(this ReadModels.Car readModel)
        {
            var dto = readModel.ConvertTo<Car>();
            dto.Owner = new CarOwner {Id = readModel.VehicleOwnerId};
            dto.Managers = readModel.ManagerIds?.Select(id => new CarManager {Id = id}).ToList();
            dto.Manufacturer = new CarManufacturer
            {
                Year = readModel.ManufactureYear,
                Make = readModel.ManufactureMake,
                Model = readModel.ManufactureModel
            };
            dto.Plate = new CarLicensePlate
                {Jurisdiction = readModel.LicenseJurisdiction, Number = readModel.LicenseNumber};
            return dto;
        }

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