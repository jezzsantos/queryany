using System;

namespace Domain.Interfaces.Entities
{
    public interface IChangeEvent
    {
        string EntityId { get; set; }

        DateTime ModifiedUtc { get; set; }
    }
}