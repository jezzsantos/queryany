using System;
using Storage.Interfaces;

namespace CarsDomain.Entities
{
    public class CarEntity : IKeyedEntity
    {
        public CarModel Model { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime OccupiedUntilUtc { get; set; }

        public string Id { get; set; }

        public string EntityName => "Car";
    }
}