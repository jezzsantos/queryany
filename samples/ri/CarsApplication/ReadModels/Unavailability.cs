using System;
using CarsDomain;
using QueryAny;
using Storage.Interfaces.ReadModels;

namespace CarsApplication.ReadModels
{
    [EntityName("Unavailability")]
    public class Unavailability : IReadModelEntity
    {
        public string CarId { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public UnavailabilityCausedBy CausedBy { get; set; }

        public string CausedByReference { get; set; }

        public string Id { get; set; }
    }
}