using System;
using System.Collections.Generic;
using System.Linq;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;

namespace CarsApplication
{
    public class CarsApplication : ApplicationBase, ICarsApplication
    {
        private readonly ILogger logger;
        private readonly IStorage<CarEntity> storage;

        public CarsApplication(ILogger logger, IStorage<CarEntity> storage)
        {
            logger.GuardAgainstNull(nameof(logger));
            storage.GuardAgainstNull(nameof(storage));
            this.logger = logger;
            this.storage = storage;
        }

        public Car Create(ICurrentCaller caller, int year, string make, string model)
        {
            caller.GuardAgainstNull(nameof(caller));

            var car = new CarEntity(this.logger);
            car.SetOwnership(Identifier.Create(caller.Id));
            car.SetManufacturer(year, make, model);

            this.storage.Add(car);

            this.logger.LogInformation("Car {Id} was created by {Caller}", car.Id, caller.Id);

            var created = this.storage.Get(car.Id);
            return created.ToCar();
        }

        public Car Occupy(ICurrentCaller caller, string id, in DateTime untilUtc)
        {
            caller.GuardAgainstNull(nameof(caller));
            id.GuardAgainstNullOrEmpty(nameof(id));

            var car = this.storage.Get(Identifier.Create(id));
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

            var query = Query.From<CarEntity>()
                .Where(e => e.OccupiedUntilUtc, ConditionOperator.LessThan, DateTime.UtcNow)
                .WithSearchOptions(searchOptions);

            var cars = this.storage.Query(query)
                .Results;

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
            dto.Id = entity.Id?.Get();
            dto.Owner = new CarOwner {Id = entity.Owner?.Id?.Get()};
            dto.Managers =
                new List<CarManager>(entity.Managers != null
                    ? entity.Managers.ManagerIds.Select(mi => new CarManager {Id = mi.Get()})
                    : Enumerable.Empty<CarManager>());

            return dto;
        }
    }
}