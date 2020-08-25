using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IStorage<CarEntity> carStorage;
        private readonly IStorage<UnavailabilityEntity> unavailabilitiesStorage;

        public CarStorage(IStorage<CarEntity> carStorage, IStorage<UnavailabilityEntity> unavailabilitiesStorage)
        {
            carStorage.GuardAgainstNull(nameof(carStorage));
            unavailabilitiesStorage.GuardAgainstNull(nameof(unavailabilitiesStorage));
            this.carStorage = carStorage;
            this.unavailabilitiesStorage = unavailabilitiesStorage;
        }

        public CarEntity Create(CarEntity car)
        {
            return this.carStorage.Add(car);
        }

        public CarEntity Get(Identifier id)
        {
            var car = this.carStorage.Get(id);

            var unavailabilities = this.unavailabilitiesStorage
                .Query(Query.From<UnavailabilityEntity>()
                    .Where(e => e.CarId, ConditionOperator.EqualTo, car.Id))
                .Results;
            unavailabilities.ForEach(u =>
                car.Unavailabilities.Add(u));

            return car;
        }

        public CarEntity Update(CarEntity car)
        {
            var updatedCar = this.carStorage.Upsert(car);

            car.Unavailabilities
                .ToList()
                .ForEach(u =>
                {
                    if (u.RequiresUpsert())
                    {
                        var updatedUnavailability = this.unavailabilitiesStorage.Upsert(u);
                        updatedCar.Unavailabilities.Add(updatedUnavailability);
                    }
                });

            return updatedCar;
        }

        public List<CarEntity> SearchAvailable(DateTime fromUtc, DateTime toUtc, SearchOptions options)
        {
            var unavailabilities = this.unavailabilitiesStorage.Query(Query.From<UnavailabilityEntity>()
                    .Where(e => e.SlotFrom, ConditionOperator.LessThanEqualTo, fromUtc)
                    .AndWhere(e => e.SlotTo, ConditionOperator.GreaterThanEqualTo, toUtc))
                .Results;

            var limit = options.Limit;
            var offset = options.Offset;
            options.ClearLimitAndOffset();

            var cars = this.carStorage.Query(Query.From<CarEntity>()
                    .WhereAll()
                    .WithSearchOptions(options))
                .Results;

            return cars
                .Where(car => !unavailabilities.Any(unavailability => unavailability.CarId.Equals(car.Id)))
                .Skip(offset)
                .Take(limit)
                .ToList();
        }
    }
}