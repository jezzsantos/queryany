using System;
using System.Collections.Generic;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces.Entities;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    [EntityName("testentities")]
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

        public ComplexNonValueType AComplexNonValueTypeValue { get; set; }

        public ComplexValueType AComplexValueTypeValue { get; set; }

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

        public static EntityFactory<TestEntity> GetFactory()
        {
            return properties => new TestEntity();
        }
    }

    [EntityName("firstjoiningtestentities")]
    public class FirstJoiningTestEntity : IPersistableEntity
    {
        public string AStringValue { get; set; }

        public int AIntValue { get; set; }

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

        public static EntityFactory<FirstJoiningTestEntity> GetFactory()
        {
            return properties => new FirstJoiningTestEntity();
        }
    }

    [EntityName("secondjoiningtestentities")]
    public class SecondJoiningTestEntity : IPersistableEntity
    {
        public string AStringValue { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int AIntValue { get; set; }

        public long ALongValue { get; set; }

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

        public static EntityFactory<SecondJoiningTestEntity> GetFactory()
        {
            return properties => new SecondJoiningTestEntity();
        }
    }

    public class ComplexNonValueType
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string APropertyValue { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }

    public class ComplexValueType : ValueType<ComplexValueType>
    {
        private ComplexValueType(string @string, int integer, bool boolean)
        {
            AStringProperty = @string;
            AnIntName = integer;
            ABooleanPropertyName = boolean;
        }

        public string AStringProperty { get; private set; }

        public int AnIntName { get; private set; }

        public bool ABooleanPropertyName { get; private set; }

        public static ComplexValueType Create(string @string, int integer, bool boolean)
        {
            return new ComplexValueType(@string, integer, boolean);
        }

        public override string Dehydrate()
        {
            return $"{AStringProperty}::{AnIntName}::{ABooleanPropertyName}";
        }

        public override void Rehydrate(string value)
        {
            if (value.HasValue())
            {
                var parts = value.Split("::");
                AStringProperty = parts[0];
                AnIntName = parts[1].HasValue()
                    ? int.Parse(parts[1])
                    : 0;
                ABooleanPropertyName = parts[2].HasValue()
                    ? bool.Parse(parts[2])
                    : false;
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {AStringProperty, AnIntName, ABooleanPropertyName};
        }
    }
}