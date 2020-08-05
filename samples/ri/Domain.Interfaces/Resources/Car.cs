using System;

namespace Domain.Interfaces.Resources
{
    public class Car : IIdentifiableResource
    {
        public CarManufacturer Manufacturer { get; set; }

        public DateTime OccupiedUntilUtc { get; set; }

        public string Id { get; set; }
    }

    public class CarManufacturer
    {
        public int Year { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }
    }
}