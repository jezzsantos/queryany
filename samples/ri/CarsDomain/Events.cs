using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;

namespace CarsDomain
{
    public static class Events
    {
        public static class Car
        {
            public class Created
            {
                public string Id { get; set; }

                public DateTime CreatedUtc { get; set; }

                public static Created Create(Identifier id)
                {
                    return new Created
                    {
                        Id = id,
                        CreatedUtc = DateTime.UtcNow
                    };
                }
            }

            public class ManufacturerChanged
            {
                public string Id { get; set; }

                public int Year { get; set; }

                public string Make { get; set; }

                public string Model { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static ManufacturerChanged Create(Identifier id, Manufacturer manufacturer)
                {
                    return new ManufacturerChanged
                    {
                        Id = id,
                        Year = manufacturer.Year,
                        Make = manufacturer.Make,
                        Model = manufacturer.Model,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class OwnershipChanged
            {
                public string Id { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public string Owner { get; set; }

                public List<string> Managers { get; set; }

                public static OwnershipChanged Create(Identifier id, CarOwner owner)
                {
                    return new OwnershipChanged
                    {
                        Id = id,
                        Owner = owner.Id,
                        Managers = new List<string> {owner.Id},
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class RegistrationChanged
            {
                public string Id { get; set; }

                public string Jurisdiction { get; set; }

                public string Number { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static RegistrationChanged Create(Identifier id, LicensePlate plate)
                {
                    return new RegistrationChanged
                    {
                        Id = id,
                        Jurisdiction = plate.Jurisdiction,
                        Number = plate.Number,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class UnavailabilitySlotAdded
            {
                public string Id { get; set; }

                public string CarId { get; set; }

                public DateTime From { get; set; }

                public DateTime To { get; set; }

                public UnavailabilityCausedBy CausedBy { get; set; }

                public string CausedByReference { get; set; }

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