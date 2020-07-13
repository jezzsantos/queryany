using System;
using System.Collections.Generic;
using QueryAny.Primitives;
using Services.Interfaces.Entities;

namespace CarsDomain.Entities
{
    public class CarEntity : EntityBase
    {
        public CarModel Model { get; private set; }

        public DateTime OccupiedUntilUtc { get; private set; }

        public override string EntityName => "Car";

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
        }
    }
}