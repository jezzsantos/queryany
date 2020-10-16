using System;
using Domain.Interfaces.Entities;

namespace Domain.Interfaces.UnitTests
{
    public class TestEvent : IChangeEvent
    {
        public string APropertyValue { get; set; }

        public string EntityId { get; set; }

        public DateTime ModifiedUtc { get; set; }
    }
}