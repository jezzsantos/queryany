using System.Collections.Generic;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    public class TestEntity : IPersistableEntity
    {
        public bool ABooleanValue { get; set; }
        public double ADoubleValue { get; set; }

        public string Id { get; private set; }

        public void Identify(string id)
        {
            Id = id;
        }

        public string EntityName => "testentities";

        public Dictionary<string, object> Dehydrate()
        {
            return this.ToObjectDictionary();
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            this.PopulateWith(properties.FromObjectDictionary<TestEntity>());
        }
    }
}