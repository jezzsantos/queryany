using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;

namespace {{DomainName | string.pascalplural}}Domain
{
    public static class Events
    {
        public static class {{DomainName | string.pascalsingular}}
        {
            public class Created : IChangeEvent
            {
                public static Created Create(Identifier id)
                {
                    return new Created
                    {
                        EntityId = id,
                        ModifiedUtc = DateTime.UtcNow,
                    };
                }

                public string EntityId { get; set; }

                public DateTime ModifiedUtc { get; set; }
            }
        }
    }
}