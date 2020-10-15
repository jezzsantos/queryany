using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;

namespace Storage.IntegrationTests
{
    [EntityName("testentities")]
    public class TestRepositoryEntity : IIdentifiableEntity, IQueryableEntity
    {
        private static int instanceCounter;

        public TestRepositoryEntity()
        {
            Id = Identifier.Create($"anid{++instanceCounter}");
        }

        public DateTime? LastPersistedAtUtc { get; set; }

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

        public Identifier Id { get; set; }
    }

    [EntityName("firstjoiningtestentities")]
    public class FirstJoiningTestQueryableEntity : IIdentifiableEntity, IQueryableEntity
    {
        private static int instanceCounter;

        public FirstJoiningTestQueryableEntity()
        {
            Id = Identifier.Create($"anid{++instanceCounter}");
        }

        public string AStringValue { get; set; }

        public int AIntValue { get; set; }

        public Identifier Id { get; set; }
    }

    [EntityName("secondjoiningtestentities")]
    public class SecondJoiningTestQueryableEntity : IIdentifiableEntity, IQueryableEntity
    {
        private static int instanceCounter;

        public SecondJoiningTestQueryableEntity()
        {
            Id = Identifier.Create($"anid{++instanceCounter}");
        }

        public string AStringValue { get; set; }

        public int AIntValue { get; set; }

        public long ALongValue { get; set; }

        public Identifier Id { get; set; }
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
                var parts = RehydrateToList(value);
                AStringProperty = parts[0];
                AnIntName = parts[1].HasValue()
                    ? int.Parse(parts[1])
                    : 0;
                ABooleanPropertyName = parts[2].HasValue()
                    ? bool.Parse(parts[2])
                    : false;
            }
        }

        private new static List<string> RehydrateToList(string hydratedValue)
        {
            if (!hydratedValue.HasValue())
            {
                return new List<string>();
            }

            return hydratedValue
                .Split("::")
                .ToList();
        }

        public static ValueObjectFactory<ComplexValueObject> Instantiate()
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