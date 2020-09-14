using System.Collections.Generic;
using QueryAny;
using Storage.Interfaces.ReadModels;

namespace CarsApplication.ReadModels
{
    [EntityName("Car")]
    public class Car : IReadModelEntity
    {
        public int ManufactureYear { get; set; }

        public string ManufactureMake { get; set; }

        public string ManufactureModel { get; set; }

        public string VehicleOwnerId { get; set; }

        public List<string> ManagerIds { get; set; }

        public string LicenseJurisdiction { get; set; }

        public string LicenseNumber { get; set; }

        public string Id { get; set; }
    }
}