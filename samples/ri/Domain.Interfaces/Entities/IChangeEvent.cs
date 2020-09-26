using System;

namespace Domain.Interfaces.Entities
{
    public interface IChangeEvent
    {
        string Id { get; set; }

        DateTime ModifiedUtc { get; set; }
    }
}