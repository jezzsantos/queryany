using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;

namespace Storage.IntegrationTests
{
    [EntityName("testentities")]
    public class TestEntity : EntityBase
    {
        public TestEntity() : this(new GuidIdentifierFactory())
        {
        }

        private TestEntity(IIdentifierFactory idFactory) : base(NullLogger.Instance, idFactory)
        {
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

        public ComplexNonValueObject AComplexNonValueObjectValue { get; set; }

        public ComplexValueObject AComplexValueObjectValue { get; set; }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(AStringValue), AStringValue);
            properties.Add(nameof(ABooleanValue), ABooleanValue);
            properties.Add(nameof(ADateTimeUtcValue), ADateTimeUtcValue);
            properties.Add(nameof(ADateTimeOffsetUtcValue), ADateTimeOffsetUtcValue);
            properties.Add(nameof(AGuidValue), AGuidValue);
            properties.Add(nameof(ADoubleValue), ADoubleValue);
            properties.Add(nameof(AIntValue), AIntValue);
            properties.Add(nameof(ALongValue), ALongValue);
            properties.Add(nameof(ABinaryValue), ABinaryValue);
            properties.Add(nameof(AComplexNonValueObjectValue), AComplexNonValueObjectValue);
            properties.Add(nameof(AComplexValueObjectValue), AComplexValueObjectValue);

            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            AStringValue = properties.GetValueOrDefault<string>(nameof(AStringValue));
            ABooleanValue = properties.GetValueOrDefault<bool>(nameof(ABooleanValue));
            ADateTimeUtcValue = properties.GetValueOrDefault<DateTime>(nameof(ADateTimeUtcValue));
            ADateTimeOffsetUtcValue = properties.GetValueOrDefault<DateTimeOffset>(nameof(ADateTimeOffsetUtcValue));
            AGuidValue = properties.GetValueOrDefault<Guid>(nameof(AGuidValue));
            ADoubleValue = properties.GetValueOrDefault<double>(nameof(ADoubleValue));
            AIntValue = properties.GetValueOrDefault<int>(nameof(AIntValue));
            ALongValue = properties.GetValueOrDefault<long>(nameof(ALongValue));
            ABinaryValue = properties.GetValueOrDefault<byte[]>(nameof(ABinaryValue));
            AComplexNonValueObjectValue =
                properties.GetValueOrDefault<ComplexNonValueObject>(nameof(AComplexNonValueObjectValue));
            AComplexValueObjectValue =
                properties.GetValueOrDefault<ComplexValueObject>(nameof(AComplexValueObjectValue));
        }

        public static EntityFactory<TestEntity> GetFactory()
        {
            return properties => new TestEntity(new HydrationIdentifierFactory(properties));
        }
    }

    [EntityName("firstjoiningtestentities")]
    public class FirstJoiningTestEntity : EntityBase
    {
        public FirstJoiningTestEntity() : this(new GuidIdentifierFactory())
        {
        }

        private FirstJoiningTestEntity(IIdentifierFactory idFactory) : base(NullLogger.Instance, idFactory)
        {
        }

        public string AStringValue { get; set; }

        public int AIntValue { get; set; }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(AStringValue), AStringValue);
            properties.Add(nameof(AIntValue), AIntValue);

            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            AStringValue = properties.GetValueOrDefault<string>(nameof(AStringValue));
            AIntValue = properties.GetValueOrDefault<int>(nameof(AIntValue));
        }

        public static EntityFactory<FirstJoiningTestEntity> GetFactory()
        {
            return properties => new FirstJoiningTestEntity(new HydrationIdentifierFactory(properties));
        }
    }

    [EntityName("secondjoiningtestentities")]
    public class SecondJoiningTestEntity : EntityBase
    {
        public SecondJoiningTestEntity() : this(new GuidIdentifierFactory())
        {
        }

        private SecondJoiningTestEntity(IIdentifierFactory idFactory) : base(NullLogger.Instance, idFactory)
        {
        }

        public string AStringValue { get; set; }

        public int AIntValue { get; set; }

        public long ALongValue { get; set; }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(AStringValue), AStringValue);
            properties.Add(nameof(AIntValue), AIntValue);
            properties.Add(nameof(ALongValue), ALongValue);

            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            AStringValue = properties.GetValueOrDefault<string>(nameof(AStringValue));
            AIntValue = properties.GetValueOrDefault<int>(nameof(AIntValue));
            ALongValue = properties.GetValueOrDefault<long>(nameof(ALongValue));
        }

        public static EntityFactory<SecondJoiningTestEntity> GetFactory()
        {
            return properties => new SecondJoiningTestEntity(new HydrationIdentifierFactory(properties));
        }
    }

    public class ComplexNonValueObject
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string APropertyValue { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }

    public class ComplexValueObject : ValueObjectBase<ComplexValueObject>
    {
        private ComplexValueObject(string @string, int integer, bool boolean)
        {
            AStringProperty = @string;
            AnIntName = integer;
            ABooleanPropertyName = boolean;
        }

        public string AStringProperty { get; private set; }

        public int AnIntName { get; private set; }

        public bool ABooleanPropertyName { get; private set; }

        public static ComplexValueObject Create(string @string, int integer, bool boolean)
        {
            return new ComplexValueObject(@string, integer, boolean);
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