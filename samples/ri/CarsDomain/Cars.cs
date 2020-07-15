using System;
using System.Collections.Generic;
using CarsDomain.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using Services.Interfaces.Entities;
using Services.Interfaces.Resources;
using ServiceStack;
using Storage.Interfaces;

namespace CarsDomain
{
    public class Cars : DomainObject, ICars
    {
        private readonly ILogger logger;
        private readonly IStorage<CarEntity> storage;

        public Cars(ILogger<Car> logger, IStorage<CarEntity> storage)
        {
            logger.GuardAgainstNull(nameof(logger));
            storage.GuardAgainstNull(nameof(storage));
            this.logger = logger;
            this.storage = storage;

            AutoMapping.RegisterPopulator((Car car, CarEntity entity) => { car.Id = entity.Id.Get(); });
        }

        public Car Create(ICurrentCaller caller, int year, string make, string model)
        {
            var carEntity = new CarEntity();
            carEntity.SetModel(year, make, model);

            var id = this.storage.Add(carEntity);
            carEntity.Identify(id);

            this.logger.LogInformation("Car {1} was created by {0}", caller.Id, id);
            return carEntity.ConvertTo<Car>();
        }

        public Car Occupy(ICurrentCaller caller, string id, in DateTime untilUtc)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var car = this.storage.Get(Identifier.Create(id));
            if (id == null)
            {
                throw new ResourceNotFoundException();
            }

            car.Occupy(untilUtc);
            this.storage.Update(car, false);

            this.logger.LogInformation("Car {1} was occupied until {2}, by {0}", caller.Id, id, untilUtc.ToIso8601());
            return car.ConvertTo<Car>();
        }

        public List<Car> SearchAvailable(ICurrentCaller caller, SearchOptions searchOptions, GetOptions getOptions)
        {
            var query = Query.From<CarEntity>()
                .Where(e => e.OccupiedUntilUtc, ConditionOperator.LessThan, DateTime.UtcNow);
            var cars = this.storage.Query(query, searchOptions).Results;

            this.logger.LogInformation("Available cars were retrieved by {0}", caller.Id);
            return cars
                .ConvertAll(c => WithGetOptions(c.ConvertTo<Car>(), getOptions));
        }

        // ReSharper disable once UnusedParameter.Local
        private static Car WithGetOptions(Car car, GetOptions options)
        {
            // TODO: expand embedded resources, etc
            return car;
        }
    }
}