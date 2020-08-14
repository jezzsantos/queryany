using System;
using System.Collections.Generic;

namespace Domain.Interfaces.Resources
{
    public class Car : IIdentifiableResource
    {
        public CarManufacturer Manufacturer { get; set; }

        public DateTime OccupiedUntilUtc { get; set; }

        public CarOwner Owner { get; set; }

        public List<CarManager> Managers { get; set; }

        public string Id { get; set; }
    }

    public class CarManager
    {
        public string Id { get; set; }
    }

    public class CarOwner
    {
        public string Id { get; set; }

        public PersonName Name { get; set; }
    }

    public class CarManufacturer
    {
        public int Year { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }
    }
}