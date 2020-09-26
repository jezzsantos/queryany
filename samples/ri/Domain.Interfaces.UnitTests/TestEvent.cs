using System;
using Domain.Interfaces.Entities;

namespace Domain.Interfaces.UnitTests
{
    public class TestEvent : IChangeEvent
    {
        public string APropertyValue { get; set; }

        public string Id { get; set; }

        public DateTime ModifiedUtc { get; set; }
    }
}