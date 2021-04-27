using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny;
using ServiceStack;

namespace Storage.IntegrationTests
{
    [EntityName("testentities")]
    public class TestRepositoryEntity : IIdentifiableEntity, IQueryableEntity
    {
        private static int instanceCounter;

        public TestRepositoryEntity()
        {
            Id = Identifier.Create($"anid{++instanceCounter:000}");
        }

        public DateTime? LastPersistedAtUtc { get; set; }

        public string AStringValue { get; set; }

        public AnEnum AnEnumValue { get; set; }

        public AnEnum? AnNullableEnumValue { get; set; }

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

    [EntityName("testentities")]
    public class TestJoinedRepositoryEntity : TestRepositoryEntity
    {
        public int AFirstIntValue { get; set; }

        public string AFirstStringValue { get; set; }
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

        public string AStringProperty { get; }

        public int AnIntName { get; }

        public bool ABooleanPropertyName { get; }

        public static ComplexValueObject Create(string @string, int integer, bool boolean)
        {
            return new ComplexValueObject(@string, integer, boolean);
        }

        public override string Dehydrate()
        {
            return $"{AStringProperty}::{AnIntName}::{ABooleanPropertyName}";
        }

        public static ValueObjectFactory<ComplexValueObject> Rehydrate()
        {
            return (value, container) =>
            {
                var parts = RehydrateToList(value);
                return new ComplexValueObject(parts[0], parts[1].ToInt(), parts[2].ToBool());
            };
        }

        private static List<string> RehydrateToList(string hydratedValue)
        {
            if (!hydratedValue.HasValue())
            {
                return new List<string>();
            }

            return hydratedValue
                .Split("::")
                .ToList();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {AStringProperty, AnIntName, ABooleanPropertyName};
        }
    }

    public enum AnEnum
    {
        None = 0,
        AValue1 = 1,
        AValue2 = 2
    }
}