using System;
using System.Collections.Generic;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace CarsDomain.Entities
{
    public class CarEntity : IPersistableEntity
    {
        public CarEntity()
        {
            CreatedOnUtc = DateTime.UtcNow;
        }

        public CarModel Model { get; private set; }

        public DateTime CreatedOnUtc { get; private set; }

        public DateTime OccupiedUntilUtc { get; private set; }

        public string Id { get; private set; }

        public string EntityName => "Car";

        public void Identify(string id)
        {
            Guard.AgainstNullOrEmpty(() => id, id);
            Id = id;
        }

        public Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(CreatedOnUtc), CreatedOnUtc},
                {nameof(Model), Model},
                {nameof(OccupiedUntilUtc), OccupiedUntilUtc}
            };
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            Id = properties.GetValueOrDefault<string>(nameof(Id));
            CreatedOnUtc = properties.GetValueOrDefault<DateTime>(nameof(CreatedOnUtc));
            Model = properties.GetValueOrDefault<CarModel>(nameof(Model));
            OccupiedUntilUtc = properties.GetValueOrDefault<DateTime>(nameof(OccupiedUntilUtc));
        }

        public void SetModel(int year, string make, string model)
        {
            Model = new CarModel(year, make, model);
        }

        public void Occupy(DateTime untilUtc)
        {
            OccupiedUntilUtc = untilUtc;
        }
    }
}