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

        protected override void When(object @event)
        {
            switch (@event)
            {
                case Events.Car.Created _:
                    break;

                case Events.Car.ManufacturerChanged changed:
                    Manufacturer = new Manufacturer(changed.Year, changed.Make, changed.Model);
                    Logger.LogDebug("Car {Id} changed manufacturer to {Year}, {Make}, {Model}", Id, changed.Year,
                        changed.Make, changed.Model);
                    break;

                case Events.Car.OwnershipChanged changed:
                    Owner = new VehicleOwner(changed.Owner);
                    Managers = new VehicleManagers();
                    Managers.Add(changed.Owner.ToIdentifier());

                    Logger.LogDebug("Car {Id} changed ownership to {Owner}", Id, Owner);
                    break;

                case Events.Car.RegistrationChanged changed:
                    Plate = new LicensePlate(changed.Jurisdiction, changed.Number);

                    Logger.LogDebug("Car {Id} registration changed to {Jurisdiction}, {Number}", Id,
                        changed.Jurisdiction, changed.Number);
                    break;

                case Events.Car.OccupancyChanged changed:
                    if (!changed.UntilUtc.HasValue())
                    {
                        throw new ArgumentOutOfRangeException(nameof(changed.UntilUtc));
                    }

                    OccupiedUntilUtc = changed.UntilUtc;
                    Logger.LogDebug("Car {Id} was occupied until {Until}", Id, changed.UntilUtc);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown event {@event.GetType()}");
            }
        }

        public void SetManufacturer(Manufacturer manufacturer)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.ManufacturerChanged.Create(Id, manufacturer));
        }

        public void SetOwnership(CarOwner owner)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.OwnershipChanged.Create(Id, owner));
        }

        public void Register(LicensePlate plate)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.RegistrationChanged.Create(Id, plate));
        }

        public void Occupy(DateTime untilUtc)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.OccupancyChanged.Create(Id, untilUtc));
        }

        protected override bool EnsureValidState()
        {
            var isValid = base.EnsureValidState();

            if (OccupiedUntilUtc.HasValue())
            {
                if (!Manufacturer.HasValue())
                {
                    throw new RuleViolationException(Resources.CarEntity_NotManufactured);
                }
                if (!Owner.HasValue())
                {
                    throw new RuleViolationException(Resources.CarEntity_NotOwned);
                }
                if (!Plate.HasValue())
                {
                    throw new RuleViolationException(Resources.CarEntity_NotRegistered);
                }
            }

            return isValid;
        }

        public static EntityFactory<CarEntity> Instantiate()
        {
            return (hydratingProperties, container) => new CarEntity(container.Resolve<ILogger>(),
                new HydrationIdentifierFactory(hydratingProperties));
        }
    }
}