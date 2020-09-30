using System.Collections.Generic;

namespace Application.Resources
{
    public class Car : IIdentifiableResource
    {
        public CarManufacturer Manufacturer { get; set; }

        public CarLicensePlate Plate { get; set; }

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
    }

    public class CarLicensePlate
    {
        public string Jurisdiction { get; set; }

        public string Number { get; set; }
    }

    public class CarManufacturer
    {
        public int Year { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }
    }
}