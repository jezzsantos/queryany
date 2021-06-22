using System;
using System.Collections.Generic;
using CarsApplication.ReadModels;
using CarsDomain;
using Common;
using Domain.Interfaces.Entities;
using Storage;
using Storage.Interfaces.ReadModels;

namespace CarsStorage
{
    public class CarEntityReadModelProjection : IReadModelProjection
    {
        private readonly IReadModelStorage<Car> carStorage;
        private readonly IRecorder recorder;
        private readonly IReadModelStorage<Unavailability> unavailabilityStorage;

        public CarEntityReadModelProjection(IRecorder recorder, IRepository repository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            repository.GuardAgainstNull(nameof(repository));

            this.recorder = recorder;
            this.carStorage = new GeneralReadModelStorage<Car>(recorder, repository);
            this.unavailabilityStorage = new GeneralReadModelStorage<Unavailability>(recorder, repository);
        }

        public Type EntityType => typeof(CarEntity);

        public bool Project(IChangeEvent originalEvent)
        {
            switch (originalEvent)
            {
                case Events.Car.Created e:
                    this.carStorage.Create(e.EntityId.ToIdentifier());
                    break;

                case Events.Car.ManufacturerChanged e:
                    this.carStorage.Update(e.EntityId, dto =>
                    {
                        dto.ManufactureYear = e.Year;
                        dto.ManufactureMake = e.Make;
                        dto.ManufactureModel = e.Model;
                    });
                    break;

                case Events.Car.OwnershipChanged e:
                    this.carStorage.Update(e.EntityId, dto =>
                    {
                        dto.VehicleOwnerId = e.Owner;
                        dto.ManagerIds = new List<string> {e.Owner};
                    });
                    break;

                case Events.Car.RegistrationChanged e:
                    this.carStorage.Update(e.EntityId, dto =>
                    {
                        dto.LicenseJurisdiction = e.Jurisdiction;
                        dto.LicenseNumber = e.Number;
                    });
                    break;

                case Events.Car.UnavailabilitySlotAdded e:
                    this.unavailabilityStorage.Create(e.EntityId, dto =>
                    {
                        dto.CarId = e.CarId;
                        dto.From = e.From;
                        dto.To = e.To;
                        dto.CausedBy = e.CausedBy;
                        dto.CausedByReference = e.CausedByReference;
                    });
                    break;

                default:
                    this.recorder.TraceDebug($"Unknown entity type '{originalEvent.GetType().Name}'");
                    return false;
            }

            return true;
        }
    }
}