using System;
using System.Collections.Generic;
using CarsApplication.ReadModels;
using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;
using Events = Domain.Interfaces.Entities.Events;

namespace CarsStorage
{
    public class CarEntityReadModelProjection : IReadModelProjection
    {
        private readonly IReadModelStorage<Car> carStorage;
        private readonly ILogger logger;
        private readonly IReadModelStorage<Unavailability> unavailabilityStorage;

        public CarEntityReadModelProjection(ILogger logger, IReadModelStorage<Car> carStorage,
            IReadModelStorage<Unavailability> unavailabilityStorage)
        {
            logger.GuardAgainstNull(nameof(logger));
            carStorage.GuardAgainstNull(nameof(carStorage));
            unavailabilityStorage.GuardAgainstNull(nameof(unavailabilityStorage));

            this.logger = logger;
            this.carStorage = carStorage;
            this.unavailabilityStorage = unavailabilityStorage;
        }

        public Type EntityType => typeof(CarEntity);

        public bool Project(object originalEvent)
        {
            switch (originalEvent)
            {
                case Events.Any.Created e:
                    this.carStorage.Create(e.Id.ToIdentifier());
                    break;

                case CarsDomain.Events.Car.ManufacturerChanged e:
                    this.carStorage.Update(e.Id, dto =>
                    {
                        dto.ManufactureYear = e.Year;
                        dto.ManufactureMake = e.Make;
                        dto.ManufactureModel = e.Model;
                    });
                    break;

                case CarsDomain.Events.Car.OwnershipChanged e:
                    this.carStorage.Update(e.Id, dto =>
                    {
                        dto.VehicleOwnerId = e.Owner;
                        dto.ManagerIds = new List<string> {e.Owner};
                    });
                    break;

                case CarsDomain.Events.Car.RegistrationChanged e:
                    this.carStorage.Update(e.Id, dto =>
                    {
                        dto.LicenseJurisdiction = e.Jurisdiction;
                        dto.LicenseNumber = e.Number;
                    });
                    break;

                case CarsDomain.Events.Car.UnavailabilitySlotAdded e:
                    this.unavailabilityStorage.Create(e.Id, dto =>
                    {
                        dto.CarId = e.CarId;
                        dto.From = e.From;
                        dto.To = e.To;
                        dto.CausedBy = e.CausedBy;
                        dto.CausedByReference = e.CausedByReference;
                    });
                    break;

                default:
                    this.logger.LogDebug($"Unknown entity type '{originalEvent.GetType().Name}'");
                    return false;
            }

            return true;
        }
    }
}