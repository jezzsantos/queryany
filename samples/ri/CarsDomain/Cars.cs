using System;
using System.Collections.Generic;
using CarsDomain.Entities;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using Services.Interfaces.Resources;
using ServiceStack;
using Storage.Interfaces;

namespace CarsDomain
{
    public class Cars : DomainObject, ICars
    {
        private readonly IStorage<CarEntity> storage;

        public Cars(IStorage<CarEntity> storage)
        {
            Guard.AgainstNull(() => storage, storage);
            this.storage = storage;
        }

        public Car Create(ICurrentCaller caller, int year, string make, string model)
        {
            var carEntity = new CarEntity();
            carEntity.SetModel(year, make, model);

            var id = this.storage.Add(carEntity);
            carEntity.Identify(id);

            return carEntity.ConvertTo<Car>();
        }

        public Car Occupy(ICurrentCaller caller, string id, in DateTime untilUtc)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            var car = this.storage.Get(id);
            if (id == null)
            {
                throw new ResourceNotFoundException();
            }

            car.Occupy(untilUtc);
            this.storage.Update(car, false);

            return car.ConvertTo<Car>();
        }

        public List<Car> SearchAvailable(ICurrentCaller caller, SearchOptions searchOptions, GetOptions getOptions)
        {
            var query = Query.From<CarEntity>()
                .Where(e => e.OccupiedUntilUtc, ConditionOperator.LessThan, DateTime.UtcNow);
            var cars = this.storage.Query(query, searchOptions).Results;

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