using System;
using System.Collections.Generic;
using Services.Interfaces.Entities;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    public class TestEntity : IPersistableEntity
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool ABooleanValue { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public double ADoubleValue { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public DateTime CreatedAtUtc { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public DateTime LastModifiedAtUtc { get; }

        public Identifier Id { get; private set; }

        public void Identify(Identifier id)
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