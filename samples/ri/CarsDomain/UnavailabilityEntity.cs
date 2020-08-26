using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace CarsDomain
{
    public class UnavailabilityEntity : EntityBase
    {
        public UnavailabilityEntity(ILogger logger, IIdentifierFactory idFactory) : base(logger,
            idFactory)
        {
        }

        public Identifier CarId { get; private set; }

        public TimeSlot Slot { get; private set; }

        public UnavailabilityCausedBy CausedBy { get; private set; }

        public string CausedByReference { get; private set; }

        public DateTime SlotFrom => Slot.From;

        public DateTime SlotTo => Slot.To;

        protected override void OnEventRaised(object @event)
        {
            switch (@event)
            {
                case Events.Car.UnavailabilitySlotAdded added:
                    CarId = added.CarId.ToIdentifier();
                    Slot = new TimeSlot(added.From, added.To);
                    CausedBy = added.CausedBy;
                    CausedByReference = added.CausedByReference;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown event {@event.GetType()}");
            }
        }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(CarId), CarId);
            properties.Add(nameof(Slot), Slot);
            properties.Add(nameof(CausedBy), CausedBy);
            properties.Add(nameof(CausedByReference), CausedByReference);

            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            CarId = properties.GetValueOrDefault<Identifier>(nameof(CarId));
            Slot = properties.GetValueOrDefault<TimeSlot>(nameof(Slot));
            CausedBy = properties.GetValueOrDefault<UnavailabilityCausedBy>(nameof(CausedBy));
            CausedByReference = properties.GetValueOrDefault<string>(nameof(CausedByReference));
        }

        public static EntityFactory<UnavailabilityEntity> Instantiate()
        {
            return (properties, container) =>
                new UnavailabilityEntity(container.Resolve<ILogger>(), new HydrationIdentifierFactory(properties));
        }
    }
}