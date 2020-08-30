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
        private readonly ICommandStorage<CarEntity> carCommandStorage;
        private readonly IQueryStorage<CarEntity> carQueryStorage;
        private readonly ICommandStorage<UnavailabilityEntity> unavailabilitiesCommandStorage;
        private readonly IQueryStorage<UnavailabilityEntity> unavailabilitiesQueryStorage;

        public CarStorage(ICommandStorage<CarEntity> carCommandStorage, IQueryStorage<CarEntity> carQueryStorage,
            ICommandStorage<UnavailabilityEntity> unavailabilitiesCommandStorage,
            IQueryStorage<UnavailabilityEntity> unavailabilitiesQueryStorage)
        {
            carCommandStorage.GuardAgainstNull(nameof(carCommandStorage));
            unavailabilitiesCommandStorage.GuardAgainstNull(nameof(unavailabilitiesCommandStorage));
            unavailabilitiesQueryStorage.GuardAgainstNull(nameof(unavailabilitiesQueryStorage));
            carQueryStorage.GuardAgainstNull(nameof(carQueryStorage));
            this.carCommandStorage = carCommandStorage;
            this.unavailabilitiesCommandStorage = unavailabilitiesCommandStorage;
            this.unavailabilitiesQueryStorage = unavailabilitiesQueryStorage;
            this.carQueryStorage = carQueryStorage;
        }

        public CarEntity Load(Identifier id)
        {
            return this.carCommandStorage.Load<CarEntity>(id);
        }

        public CarEntity Save(CarEntity car)
        {
            this.carCommandStorage.Save(car);

            var updatedCar = this.carCommandStorage.Upsert(car);

            car.Unavailabilities
                .ToList()
                .ForEach(u =>
                {
                    if (u.RequiresUpsert())
                    {
                        var updatedUnavailability = this.unavailabilitiesCommandStorage.Upsert(u);
                        updatedCar.Unavailabilities.Add(updatedUnavailability);
                    }
                });

            return updatedCar;
        }

        public List<CarEntity> SearchAvailable(DateTime fromUtc, DateTime toUtc, SearchOptions options)
        {
            var unavailabilities = this.unavailabilitiesQueryStorage.Query(Query.From<UnavailabilityEntity>()
                    .Where(e => e.SlotFrom, ConditionOperator.LessThanEqualTo, fromUtc)
                    .AndWhere(e => e.SlotTo, ConditionOperator.GreaterThanEqualTo, toUtc))
                .Results;

            var limit = options.Limit;
            var offset = options.Offset;
            options.ClearLimitAndOffset();

            var cars = this.carQueryStorage.Query(Query.From<CarEntity>()
                    .WhereAll()
                    .WithSearchOptions(options))
                .Results;

            return cars
                .Where(car => unavailabilities.All(unavailability => unavailability.CarId != car.Id))
                .Skip(offset)
                .Take(limit)
                .ToList();
        }
    }
}