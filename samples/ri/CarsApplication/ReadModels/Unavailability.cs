using System;
using Application.Storage.Interfaces.ReadModels;
using CarsDomain;
using QueryAny;

namespace CarsApplication.ReadModels
{
    [EntityName("Unavailability")]
    public class Unavailability : ReadModelEntity
    {
        public string CarId { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public UnavailabilityCausedBy CausedBy { get; set; }

        public string CausedByReference { get; set; }
    }
}