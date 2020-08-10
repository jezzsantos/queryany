using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
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
            logger.GuardAgainstNull(nameof(logger));
        }

        public Manufacturer Manufacturer { get; private set; }

        public VehicleOwner Owner { get; private set; }

        public VehicleManagers Managers { get; private set; }

        public DateTime OccupiedUntilUtc { get; private set; }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(Manufacturer), Manufacturer);
            properties.Add(nameof(OccupiedUntilUtc), OccupiedUntilUtc);
            properties.Add(nameof(Owner), Owner);
            properties.Add(nameof(Managers), Managers);

            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            Manufacturer = properties.GetValueOrDefault<Manufacturer>(nameof(Manufacturer));
            OccupiedUntilUtc = properties.GetValueOrDefault<DateTime>(nameof(OccupiedUntilUtc));
            Owner = properties.GetValueOrDefault<VehicleOwner>(nameof(Owner));
            Managers = properties.GetValueOrDefault<VehicleManagers>(nameof(Managers));
        }

        public void SetManufacturer(int year, string make, string model)
        {
            Manufacturer = new Manufacturer(year, make, model);
        }

        public void SetOwnership(Identifier ownerId)
        {
            Owner = new VehicleOwner(ownerId);
            Managers = new VehicleManagers();
            Managers.Add(ownerId);
        }

        public void Occupy(DateTime untilUtc)
        {
            if (!untilUtc.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(untilUtc));
            }

            OccupiedUntilUtc = untilUtc;
            Logger.LogDebug("Car was occupied until {Until}", untilUtc);
        }

        public static EntityFactory<CarEntity> GetFactory(ILogger logger)
        {
            return properties => new CarEntity(logger, new HydrationIdentifierFactory(properties));
        }
    }
}