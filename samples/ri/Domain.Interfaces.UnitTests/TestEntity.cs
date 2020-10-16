using System;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Domain.Interfaces.UnitTests
{
    public class TestEntity : EntityBase
    {
        public TestEntity(ILogger logger, IIdentifierFactory idFactory)
            : base(logger, idFactory)
        {
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string APropertyName { get; private set; }

        public void ChangeProperty(string value)
        {
            RaiseChangeEvent(new ChangeEvent {APropertyName = value});
        }

        protected override void OnEventRaised(IChangeEvent @event)
        {
            //Not used in testing
        }

        public class ChangeEvent : IChangeEvent
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public string APropertyName { get; set; }

            public string EntityId { get; set; }

            public DateTime ModifiedUtc { get; set; }
        }
    }

    public class NullIdentifierFactory : IIdentifierFactory
    {
        public Identifier Create(IIdentifiableEntity entity)
        {
            return null;
        }

        public bool IsValid(Identifier value)
        {
            throw new NotImplementedException();
        }
    }
}