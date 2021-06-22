using Common;
using Domain.Interfaces.Entities;

namespace Application.Interfaces.UnitTests.Storage
{
    public class TestEntity : EntityBase
    {
        public TestEntity(IRecorder recorder, IIdentifierFactory idFactory)
            : base(recorder, idFactory)
        {
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string APropertyName { get; private set; }

        protected override void OnEventRaised(IChangeEvent @event)
        {
            //Not used in testing
        }
    }
}