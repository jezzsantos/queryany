using System;

namespace Services.Interfaces.Resources
{
    public class Car : IIdentifiableResource
    {
        public CarModel Model { get; set; }

        public DateTime OccupiedUntilUtc { get; set; }
        public string Id { get; set; }
    }

    public class CarModel
    {
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
    }
}