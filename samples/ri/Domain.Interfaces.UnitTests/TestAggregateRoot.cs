using System;
using Common;
using Domain.Interfaces.Entities;

namespace Domain.Interfaces.UnitTests
{
    public class TestAggregateRoot : AggregateRootBase
    {
        public TestAggregateRoot(IRecorder recorder, IIdentifierFactory idFactory)
            : base(recorder, idFactory, id => new CreateEvent {EntityId = id})
        {
        }

        private TestAggregateRoot(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier)
            : base(recorder, idFactory, identifier)
        {
        }

        public new long ChangeVersion => base.ChangeVersion;

        public void ChangeProperty(string value)
        {
            RaiseChangeEvent(new ChangeEvent {APropertyName = value});
        }

        protected override void OnStateChanged(IChangeEvent @event)
        {
            //Not used in testing
        }

        public static AggregateRootFactory<TestAggregateRoot> Rehydrate()
        {
            return (identifier, container, rehydratingProperties) => new TestAggregateRoot(
                container.Resolve<IRecorder>(),
                container.Resolve<IIdentifierFactory>(), identifier);
        }

        public class CreateEvent : IChangeEvent
        {
            public string EntityId { get; set; }

            public DateTime ModifiedUtc { get; set; }
        }

        public class ChangeEvent : IChangeEvent
        {
            public string APropertyName { get; set; }

            public string EntityId { get; set; }

            public DateTime ModifiedUtc { get; set; }
        }
    }
}