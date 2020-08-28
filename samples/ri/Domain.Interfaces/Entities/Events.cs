using System;

namespace Domain.Interfaces.Entities
{
    public static class Events
    {
        public static class Any
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
        }
    }
}