using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;

namespace CarsDomain
{
    public static class Events
    {
        public static class Car
        {
            public class Created : IChangeEvent
            {
                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static Created Create(Identifier id)
                {
                    return new Created
                    {
                        EntityId = id,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class ManufacturerChanged : IChangeEvent
            {
                public int Year { get; set; }

                public string Make { get; set; }

                public string Model { get; set; }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static ManufacturerChanged Create(Identifier id, Manufacturer manufacturer)
                {
                    return new ManufacturerChanged
                    {
                        EntityId = id,
                        Year = manufacturer.Year,
                        Make = manufacturer.Make,
                        Model = manufacturer.Model,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class OwnershipChanged : IChangeEvent
            {
                public string Owner { get; set; }

                public List<string> Managers { get; set; }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static OwnershipChanged Create(Identifier id, VehicleOwner owner)
                {
                    return new OwnershipChanged
                    {
                        EntityId = id,
                        Owner = owner.OwnerId,
                        Managers = new List<string> {owner.OwnerId},
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class RegistrationChanged : IChangeEvent
            {
                public string Jurisdiction { get; set; }

                public string Number { get; set; }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static RegistrationChanged Create(Identifier id, LicensePlate plate)
                {
                    return new RegistrationChanged
                    {
                        EntityId = id,
                        Jurisdiction = plate.Jurisdiction,
                        Number = plate.Number,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class UnavailabilitySlotAdded : IChangeEvent
            {
                public string CarId { get; set; }

                public DateTime From { get; set; }

                public DateTime To { get; set; }

                public UnavailabilityCausedBy CausedBy { get; set; }

                public string CausedByReference { get; set; }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static UnavailabilitySlotAdded Create(Identifier carId, TimeSlot slot,
                    UnavailabilityCausedBy causedBy, string causedById)
                {
                    return new UnavailabilitySlotAdded
                    {
                        CarId = carId,
                        From = slot.From,
                        To = slot.To,
                        CausedBy = causedBy,
                        CausedByReference = causedById,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }
        }
    }
}