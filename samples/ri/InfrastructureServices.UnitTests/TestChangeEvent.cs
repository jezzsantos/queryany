using System;
using Domain.Interfaces.Entities;

namespace InfrastructureServices.UnitTests
{
    public class TestChangeEvent : IChangeEvent
    {
        public string EntityId { get; set; }

        public DateTime ModifiedUtc { get; set; }
    }
}