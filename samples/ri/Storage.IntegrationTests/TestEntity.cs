using System;
using System.Collections.Generic;
using Services.Interfaces.Entities;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    public class TestEntity : IPersistableEntity
    {
        public TestEntity()
        {
        }

        public TestEntity(Identifier id)
        {
            Id = id;
        }

        public string AStringValue { get; set; }
        public bool ABooleanValue { get; set; }
        public DateTime ADateTimeUtcValue { get; set; }
        public DateTimeOffset ADateTimeOffsetUtcValue { get; set; }
        public double ADoubleValue { get; set; }
        public Guid AGuidValue { get; set; }
        public int AIntValue { get; set; }
        public long ALongValue { get; set; }
        public byte[] ABinaryValue { get; set; }
        public ComplexType AComplexTypeValue { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastModifiedAtUtc { get; set; }

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

    public class FirstJoiningTestEntity : IPersistableEntity
    {
        public string AStringValue { get; set; }
        public int AIntValue { get; set; }

        public string EntityName => "firstjoiningtestentities";

        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastModifiedAtUtc { get; set; }
        public Identifier Id { get; private set; }

        public void Identify(Identifier id)
        {
            Id = id;
        }

        public Dictionary<string, object> Dehydrate()
        {
            return this.ToObjectDictionary();
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            this.PopulateWith(properties.FromObjectDictionary<TestEntity>());
        }
    }

    public class SecondJoiningTestEntity : IPersistableEntity
    {
        public string AStringValue { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int AIntValue { get; set; }

        public long ALongValue { get; set; }

        public string EntityName => "secondjoiningtestentities";

        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastModifiedAtUtc { get; set; }
        public Identifier Id { get; private set; }

        public void Identify(Identifier id)
        {
            Id = id;
        }

        public Dictionary<string, object> Dehydrate()
        {
            return this.ToObjectDictionary();
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            this.PopulateWith(properties.FromObjectDictionary<TestEntity>());
        }
    }

    public class ComplexType
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string APropertyValue { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}