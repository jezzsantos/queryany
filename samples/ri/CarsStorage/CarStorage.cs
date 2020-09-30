using System;
using System.Collections.Generic;
using System.Linq;
using CarsApplication.ReadModels;
using CarsApplication.Storage;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Storage;
using Storage.Interfaces;

namespace CarsStorage
{
    public class CarStorage : ICarStorage
    {
        private readonly IEventStreamStorage<CarEntity> carEventStreamStorage;
        private readonly IQueryStorage<Car> carQueryStorage;
        private readonly IQueryStorage<Unavailability> unavailabilitiesQueryStorage;

        public CarStorage(ILogger logger, IDomainFactory domainFactory,
            IEventStreamStorage<CarEntity> eventStreamStorage,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            eventStreamStorage.GuardAgainstNull(nameof(eventStreamStorage));
            repository.GuardAgainstNull(nameof(repository));

            this.carQueryStorage = new GeneralQueryStorage<Car>(logger, domainFactory, repository);
            this.carEventStreamStorage = eventStreamStorage;
            this.unavailabilitiesQueryStorage =
                new GeneralQueryStorage<Unavailability>(logger, domainFactory, repository);
        }

        public CarStorage(IQueryStorage<Car> carQueryStorage,
            IEventStreamStorage<CarEntity> carEventStreamStorage,
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