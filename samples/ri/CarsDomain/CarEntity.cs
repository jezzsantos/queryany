using System;
using CarsDomain.Properties;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;

namespace CarsDomain
{
    [EntityName("Car")]
    public class CarEntity : AggregateRootBase
    {
        public CarEntity(ILogger logger, IIdentifierFactory idFactory) : base(logger, idFactory,
            CarsDomain.Events.Car.Created.Create)
        {
        }

        private CarEntity(ILogger logger, IIdentifierFactory idFactory, Identifier identifier) : base(logger, idFactory,
            identifier)
        {
        }

        public Manufacturer Manufacturer { get; private set; }

        public VehicleOwner Owner { get; private set; }

        public VehicleManagers Managers { get; private set; }

        public LicensePlate Plate { get; private set; }

        public Unavailabilities Unavailabilities { get; } = new Unavailabilities();

        protected override void OnStateChanged(IChangeEvent @event)
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

                case Events.Car.UnavailabilitySlotAdded added:
                    var unavailability = new UnavailabilityEntity(Logger, IdFactory);
                    added.EntityId = unavailability.Id;
                    unavailability.SetAggregateEventHandler(RaiseChangeEvent);
                    RaiseToEntity(unavailability, @event);
                    Unavailabilities.Add(unavailability);
                    Logger.LogDebug("Car {Id} had been made unavailable from {From} until {To}", Id, added.From,
                        added.To);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown event {@event.GetType()}");
            }
        }

        public void SetManufacturer(Manufacturer manufacturer)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.ManufacturerChanged.Create(Id, manufacturer));
        }

        public void SetOwnership(VehicleOwner owner)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.OwnershipChanged.Create(Id, owner));
        }

        public void Register(LicensePlate plate)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.RegistrationChanged.Create(Id, plate));
        }

        public void Offline(TimeSlot slot)
        {
            RaiseChangeEvent(CarsDomain.Events.Car.UnavailabilitySlotAdded.Create(Id, slot,
                UnavailabilityCausedBy.Offline, null));
        }

        public void AddUnavailability(TimeSlot slot, UnavailabilityCausedBy causedBy, string causedByReference)
        {
            RaiseChangeEvent(
                CarsDomain.Events.Car.UnavailabilitySlotAdded.Create(Id, slot, causedBy, causedByReference));
        }

        protected override bool EnsureValidState()
        {
            var isValid = base.EnsureValidState();

            Unavailabilities.EnsureValidState();

            if (Unavailabilities.Count > 0)
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

        public static AggregateRootFactory<CarEntity> Instantiate()
        {
            return (identifier, container, rehydratingProperties) => new CarEntity(container.Resolve<ILogger>(),
                container.Resolve<IIdentifierFactory>(), identifier);
        }
    }
}