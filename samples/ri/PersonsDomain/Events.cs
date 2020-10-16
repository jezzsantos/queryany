using System;
using Domain.Interfaces.Entities;

namespace PersonsDomain
{
    public static class Events
    {
        public static class Person
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

            public class PhoneNumberChanged : IChangeEvent
            {
                public string PhoneNumber { get; set; }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static PhoneNumberChanged Create(Identifier id, PhoneNumber number)
                {
                    return new PhoneNumberChanged
                    {
                        EntityId = id,
                        PhoneNumber = number,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class EmailChanged : IChangeEvent
            {
                public string EmailAddress { get; set; }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static EmailChanged Create(Identifier id, Email email)
                {
                    return new EmailChanged
                    {
                        EntityId = id,
                        EmailAddress = email,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class NameChanged : IChangeEvent
            {
                public string FirstName { get; set; }

                public string LastName { get; set; }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static NameChanged Create(Identifier id, PersonName name)
                {
                    return new NameChanged
                    {
                        EntityId = id,
                        FirstName = name.FirstName,
                        LastName = name.LastName,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }

            public class DisplayNameChanged : IChangeEvent
            {
                public string DisplayName { get; set; }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }

                public static DisplayNameChanged Create(Identifier id, PersonDisplayName name)
                {
                    return new DisplayNameChanged
                    {
                        EntityId = id,
                        DisplayName = name,
                        ModifiedUtc = DateTime.UtcNow
                    };
                }
            }
        }
    }
}