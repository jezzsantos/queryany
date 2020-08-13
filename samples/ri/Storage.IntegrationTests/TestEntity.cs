﻿using System;
using System.Collections.Generic;
using Domain.Interfaces;
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

        public bool? ANullableBooleanValue { get; set; }

        public DateTime ADateTimeUtcValue { get; set; }

        public DateTime? ANullableDateTimeUtcValue { get; set; }

        public DateTimeOffset ADateTimeOffsetValue { get; set; }

        public DateTimeOffset? ANullableDateTimeOffsetValue { get; set; }

        public double ADoubleValue { get; set; }

        public double? ANullableDoubleValue { get; set; }

        public Guid AGuidValue { get; set; }

        public Guid? ANullableGuidValue { get; set; }

        public int AIntValue { get; set; }

        public int? ANullableIntValue { get; set; }

        public long ALongValue { get; set; }

        public long? ANullableLongValue { get; set; }

        public byte[] ABinaryValue { get; set; }

        public ComplexNonValueObject AComplexNonValueObjectValue { get; set; }

        public ComplexValueObject AComplexValueObjectValue { get; set; }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(AStringValue), AStringValue);
            properties.Add(nameof(ABooleanValue), ABooleanValue);
            properties.Add(nameof(ANullableBooleanValue), ANullableBooleanValue);
            properties.Add(nameof(ADateTimeUtcValue), ADateTimeUtcValue);
            properties.Add(nameof(ANullableDateTimeUtcValue), ANullableDateTimeUtcValue);
            properties.Add(nameof(ADateTimeOffsetValue), ADateTimeOffsetValue);
            properties.Add(nameof(ANullableDateTimeOffsetValue), ANullableDateTimeOffsetValue);
            properties.Add(nameof(AGuidValue), AGuidValue);
            properties.Add(nameof(ANullableGuidValue), ANullableGuidValue);
            properties.Add(nameof(ADoubleValue), ADoubleValue);
            properties.Add(nameof(ANullableDoubleValue), ANullableDoubleValue);
            properties.Add(nameof(AIntValue), AIntValue);
            properties.Add(nameof(ANullableIntValue), ANullableIntValue);
            properties.Add(nameof(ALongValue), ALongValue);
            properties.Add(nameof(ANullableLongValue), ANullableLongValue);
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
            ANullableBooleanValue = properties.GetValueOrDefault<bool?>(nameof(ANullableBooleanValue));
            ADateTimeUtcValue = properties.GetValueOrDefault<DateTime>(nameof(ADateTimeUtcValue));
            ANullableDateTimeUtcValue = properties.GetValueOrDefault<DateTime?>(nameof(ANullableDateTimeUtcValue));
            ADateTimeOffsetValue = properties.GetValueOrDefault<DateTimeOffset>(nameof(ADateTimeOffsetValue));
            ANullableDateTimeOffsetValue =
                properties.GetValueOrDefault<DateTimeOffset?>(nameof(ANullableDateTimeOffsetValue));
            AGuidValue = properties.GetValueOrDefault<Guid>(nameof(AGuidValue));
            ANullableGuidValue = properties.GetValueOrDefault<Guid?>(nameof(ANullableGuidValue));
            ADoubleValue = properties.GetValueOrDefault<double>(nameof(ADoubleValue));
            ANullableDoubleValue = properties.GetValueOrDefault<double?>(nameof(ANullableDoubleValue));
            AIntValue = properties.GetValueOrDefault<int>(nameof(AIntValue));
            ANullableIntValue = properties.GetValueOrDefault<int?>(nameof(ANullableIntValue));
            ALongValue = properties.GetValueOrDefault<long>(nameof(ALongValue));
            ANullableLongValue = properties.GetValueOrDefault<long?>(nameof(ANullableLongValue));
            ABinaryValue = properties.GetValueOrDefault<byte[]>(nameof(ABinaryValue));
            AComplexNonValueObjectValue =
                properties.GetValueOrDefault<ComplexNonValueObject>(nameof(AComplexNonValueObjectValue));
            AComplexValueObjectValue =
                properties.GetValueOrDefault<ComplexValueObject>(nameof(AComplexValueObjectValue));
        }

        public static EntityFactory<TestEntity> Rehydrate()
        {
            return (properties, container) => new TestEntity(new HydrationIdentifierFactory(properties));
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

        public static EntityFactory<FirstJoiningTestEntity> Rehydrate()
        {
            return (properties, container) => new FirstJoiningTestEntity(new HydrationIdentifierFactory(properties));
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

        public static EntityFactory<SecondJoiningTestEntity> Rehydrate()
        {
            return (properties, container) => new SecondJoiningTestEntity(new HydrationIdentifierFactory(properties));
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

        public static ValueObjectFactory<ComplexValueObject> Rehydrate()
        {
            return (value, container) =>
            {
                var parts = RehydrateToList(value);
                return new ComplexValueObject(parts[0], parts[1].ToInt(), parts[2].ToBool());
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {AStringProperty, AnIntName, ABooleanPropertyName};
        }
    }
}