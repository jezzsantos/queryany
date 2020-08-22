using System;
using System.Collections.Generic;
using CarsDomain.Properties;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;

namespace CarsDomain
{
    [EntityName("Car")]
    public class CarEntity : EntityBase
    {
        public CarEntity(ILogger logger, IIdentifierFactory idFactory) : base(logger, idFactory)
        {
            RaiseCreateEvent(CarsDomain.Events.Car.Created.Create(Id));
        }

        public Manufacturer Manufacturer { get; private set; }

        public VehicleOwner Owner { get; private set; }

        public VehicleManagers Managers { get; private set; }

        public LicensePlate Plate { get; private set; }

        public DateTime OccupiedUntilUtc { get; private set; }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(Manufacturer), Manufacturer);
            properties.Add(nameof(OccupiedUntilUtc), OccupiedUntilUtc);
            properties.Add(nameof(Owner), Owner);
            properties.Add(nameof(Managers), Managers);
            properties.Add(nameof(Plate), Plate);

            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            Manufacturer = properties.GetValueOrDefault<Manufacturer>(nameof(Manufacturer));
            OccupiedUntilUtc = properties.GetValueOrDefault<DateTime>(nameof(OccupiedUntilUtc));
            Owner = properties.GetValueOrDefault<VehicleOwner>(nameof(Owner));
            Managers = properties.GetValueOrDefault<VehicleManagers>(nameof(Managers));
            Plate = properties.GetValueOrDefault<LicensePlate>(nameof(Plate));
        }

        public void SetManufacturer(int year, string make, string model)
        {
            Manufacturer = new Manufacturer(year, make, model);

            Logger.LogDebug("Car {Id} changed manufacturer to {Year}, {Make}, {Model}", Id, year, make, model);
            RaiseChangeEvent(CarsDomain.Events.Car.ManufacturerChanged.Create(Id, Manufacturer));
        }

        public void SetOwnership(CarOwner owner)
        {
            Owner = new VehicleOwner(owner);
            Managers = new VehicleManagers();
            Managers.Add(owner.Id.ToIdentifier());

            Logger.LogDebug("Car {Id} changed ownership to {Owner}", Id, Owner);
            RaiseChangeEvent(CarsDomain.Events.Car.OwnershipChanged.Create(Id, Owner, Managers));
        }

        public void Register(string jurisdiction, string number)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.OwnershipChanged.Create(Id, owner));
        }

            Logger.LogDebug("Car {Id} registration changed to {Jurisdiction}, {Number}", Id, jurisdiction, number);
            RaiseChangeEvent(CarsDomain.Events.Car.RegistrationChanged.Create(Id, Plate));
        }

        public void Occupy(DateTime untilUtc)
        {
            if (!untilUtc.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(untilUtc));
            }

            OccupiedUntilUtc = untilUtc;

            Logger.LogDebug("Car {Id} was occupied until {Until}", Id, untilUtc);
            RaiseChangeEvent(CarsDomain.Events.Car.OccupancyChanged.Create(Id, untilUtc));
        }

        public static EntityFactory<CarEntity> Instantiate()
        {
            return (hydratingProperties, container) => new CarEntity(container.Resolve<ILogger>(),
                new HydrationIdentifierFactory(hydratingProperties));
        }
    }
}