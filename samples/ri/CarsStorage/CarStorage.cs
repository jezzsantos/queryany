﻿using System;
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
        private readonly ICommandStorage<CarEntity> carCommandStorage;
        private readonly IEventingStorage<CarEntity> carEventingStorage;
        private readonly IQueryStorage<Car> carQueryStorage;
        private readonly ICommandStorage<UnavailabilityEntity> unavailabilitiesCommandStorage;
        private readonly IQueryStorage<Unavailability> unavailabilitiesQueryStorage;

        public CarStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));

            this.carCommandStorage = new GeneralCommandStorage<CarEntity>(logger, domainFactory, repository);
            this.carQueryStorage = new GeneralQueryStorage<Car>(logger, domainFactory, repository);
            this.carEventingStorage = new GeneralEventingStorage<CarEntity>(logger, domainFactory, repository);
            this.unavailabilitiesCommandStorage =
                new GeneralCommandStorage<UnavailabilityEntity>(logger, domainFactory, repository);
            this.unavailabilitiesQueryStorage =
                new GeneralQueryStorage<Unavailability>(logger, domainFactory, repository);
        }

        public CarStorage(ICommandStorage<CarEntity> carCommandStorage,
            IQueryStorage<Car> carQueryStorage,
            IEventingStorage<CarEntity> carEventingStorage,
            ICommandStorage<UnavailabilityEntity> unavailabilitiesCommandStorage,
            IQueryStorage<Unavailability> unavailabilitiesQueryStorage)
        {
            carCommandStorage.GuardAgainstNull(nameof(carCommandStorage));
            carQueryStorage.GuardAgainstNull(nameof(carQueryStorage));
            carEventingStorage.GuardAgainstNull(nameof(carEventingStorage));
            unavailabilitiesCommandStorage.GuardAgainstNull(nameof(unavailabilitiesCommandStorage));
            unavailabilitiesQueryStorage.GuardAgainstNull(nameof(unavailabilitiesQueryStorage));
            this.carCommandStorage = carCommandStorage;
            this.carQueryStorage = carQueryStorage;
            this.carEventingStorage = carEventingStorage;
            this.unavailabilitiesCommandStorage = unavailabilitiesCommandStorage;
            this.unavailabilitiesQueryStorage = unavailabilitiesQueryStorage;
        }

        public CarEntity Load(Identifier id)
        {
            return this.carEventingStorage.Load(id);
        }

        public CarEntity Save(CarEntity car)
        {
            this.carEventingStorage.Save(car);

            //TODO: replace with ReadModel
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