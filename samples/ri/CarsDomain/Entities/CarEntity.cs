using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces.Entities;
using Storage.Interfaces;

namespace CarsDomain.Entities
{
    [EntityName("Car")]
    public class CarEntity : EntityBase
    {
        private readonly ILogger logger;

        public CarEntity(ILogger logger)
        {
            logger.GuardAgainstNull(nameof(logger));
            this.logger = logger;
        }

        public CarModel Model { get; private set; }

        public DateTime OccupiedUntilUtc { get; private set; }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(Model), Model);
            properties.Add(nameof(OccupiedUntilUtc), OccupiedUntilUtc);

            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            Model = properties.GetValueOrDefault<CarModel>(nameof(Model));
            OccupiedUntilUtc = properties.GetValueOrDefault<DateTime>(nameof(OccupiedUntilUtc));
        }

        public void SetModel(int year, string make, string model)
        {
            Model = new CarModel(year, make, model);
        }

        public void Occupy(DateTime untilUtc)
        {
            if (!untilUtc.HasValue())
            {
                throw new ArgumentOutOfRangeException(nameof(untilUtc));
            }

            OccupiedUntilUtc = untilUtc;
            this.logger.LogDebug("Car was occupied until {Until}", untilUtc);
        }


        public static EntityFactory<CarEntity> GetFactory(ILogger logger)
        {
            return properties => new CarEntity(logger);
        }
    }
}