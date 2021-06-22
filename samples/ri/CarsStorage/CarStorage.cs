using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Application.Interfaces.Storage;
using Application.Storage.Interfaces;
using CarsApplication.ReadModels;
using CarsApplication.Storage;
using CarsDomain;
using Common;
using Domain.Interfaces.Entities;
using QueryAny;
using Storage;

namespace CarsStorage
{
    public class CarStorage : ICarStorage
    {
        private readonly IEventStreamStorage<CarEntity> carEventStreamStorage;
        private readonly IQueryStorage<Car> carQueryStorage;
        private readonly IQueryStorage<Unavailability> unavailabilitiesQueryStorage;

        public CarStorage(IRecorder recorder, IDomainFactory domainFactory,
            IEventStreamStorage<CarEntity> eventStreamStorage,
            IRepository repository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            eventStreamStorage.GuardAgainstNull(nameof(eventStreamStorage));
            repository.GuardAgainstNull(nameof(repository));

            this.carQueryStorage = new GeneralQueryStorage<Car>(recorder, domainFactory, repository);
            this.carEventStreamStorage = eventStreamStorage;
            this.unavailabilitiesQueryStorage =
                new GeneralQueryStorage<Unavailability>(recorder, domainFactory, repository);
        }

        public CarStorage(IEventStreamStorage<CarEntity> carEventStreamStorage,
            IQueryStorage<Car> carQueryStorage,
            IQueryStorage<Unavailability> unavailabilitiesQueryStorage)
        {
            carQueryStorage.GuardAgainstNull(nameof(carQueryStorage));
            carEventStreamStorage.GuardAgainstNull(nameof(carEventStreamStorage));
            unavailabilitiesQueryStorage.GuardAgainstNull(nameof(unavailabilitiesQueryStorage));
            this.carQueryStorage = carQueryStorage;
            this.carEventStreamStorage = carEventStreamStorage;
            this.unavailabilitiesQueryStorage = unavailabilitiesQueryStorage;
        }

        public CarEntity Load(Identifier id)
        {
            return this.carEventStreamStorage.Load(id);
        }

        public CarEntity Save(CarEntity car)
        {
            this.carEventStreamStorage.Save(car);
            return car;
        }

        public List<Car> SearchAvailable(DateTime fromUtc, DateTime toUtc, SearchOptions options)
        {
            var unavailabilities = this.unavailabilitiesQueryStorage.Query(Query.From<Unavailability>()
                    .Where(e => e.From, ConditionOperator.LessThanEqualTo, fromUtc)
                    .AndWhere(e => e.To, ConditionOperator.GreaterThanEqualTo, toUtc))
                .Results;

            var limit = options.Limit;
            var offset = options.Offset;
            options.ClearLimitAndOffset();

            var cars = this.carQueryStorage.Query(Query.From<Car>()
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