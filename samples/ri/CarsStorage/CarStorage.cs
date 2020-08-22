using System;
using System.Collections.Generic;
using CarsApplication.Storage;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace CarsStorage
{
    public class CarStorage : ICarStorage
    {
        private readonly IStorage<CarEntity> storage;

        public CarStorage(IStorage<CarEntity> storage)
        {
            storage.GuardAgainstNull(nameof(storage));
            this.storage = storage;
        }

        public CarEntity Create(CarEntity car)
        {
            return this.storage.Add(car);
        }

        public CarEntity Get(Identifier id)
        {
            return this.storage.Get(id);
        }

        public CarEntity Update(CarEntity car)
        {
            return this.storage.Update(car);
        }

        public List<CarEntity> SearchAvailable(SearchOptions options)
        {
            var query = Query.From<CarEntity>()
                .Where(e => e.OccupiedUntilUtc, ConditionOperator.GreaterThan, DateTime.UtcNow)
                .WithSearchOptions(options);

            return this.storage.Query(query)
                .Results;
        }
    }
}