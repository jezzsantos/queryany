using System;
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

            AutoMapping.RegisterPopulator((Car car, CarEntity entity) => { car.Id = entity.Id?.Get(); });
        }

        public Car Create(ICurrentCaller caller, int year, string make, string model)
        {
            caller.GuardAgainstNull(nameof(caller));

            var carEntity = new CarEntity(this.logger);
            carEntity.SetManufacturer(year, make, model);

            this.storage.Add(carEntity);

            this.logger.LogInformation("Car {Id} was created by {Caller}", carEntity.Id, caller.Id);

            return carEntity.ConvertTo<Car>();
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
            this.storage.Update(car);

            this.logger.LogInformation("Car {Id} was occupied until {Until}, by {Caller}", id, untilUtc, caller.Id);

            return car.ConvertTo<Car>();
        }

        public SearchResults<Car> SearchAvailable(ICurrentCaller caller, SearchOptions searchOptions,
            GetOptions getOptions)
        {
            caller.GuardAgainstNull(nameof(caller));

            var query = Query.From<CarEntity>()
                .Where(e => e.OccupiedUntilUtc, ConditionOperator.LessThan, DateTime.UtcNow)
                .WithSearchOptions(searchOptions);

            var cars = this.storage.Query(query, searchOptions)
                .Results;

            this.logger.LogInformation("Available carsApplication were retrieved by {Caller}", caller.Id);

            return searchOptions.ApplyWithMetadata(cars
                .ConvertAll(c => WithGetOptions(c.ConvertTo<Car>(), getOptions)));
        }

        // ReSharper disable once UnusedParameter.Local
        private static Car WithGetOptions(Car car, GetOptions options)
        {
            // TODO: expand embedded resources, etc
            return car;
        }
    }
}