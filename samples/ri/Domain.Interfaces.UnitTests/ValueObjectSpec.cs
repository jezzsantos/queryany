using System.Collections.Generic;
using System.Linq;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using ServiceStack;
using Xunit;

namespace Domain.Interfaces.UnitTests
{
    [Trait("Category", "Unit")]
    public class ValueObjectSpec
    {
        [Fact]
        public void WhenDeserialized_ThenReturnsInstance()
        {
            var result = typeof(TestSingleStringValueObject).CreateInstance();

            result.Should().NotBeNull();
        }

        [Fact]
        public void WhenDehydrateSinglePropertyValue_ThenReturnsProperties()
        {
            var valueObject = new TestSingleStringValueObject("avalue");
            var result = valueObject.Dehydrate();

            result.Should().Be("avalue");
        }

        [Fact]
        public void WhenDehydrateMultiPropertyValueWithNulls_ThenReturnsProperties()
        {
            var valueObject = new TestMultiValueObject(null, 25, true);
            var result = valueObject.Dehydrate();

            result.Should().Be("{\"Val1\":\"NULL\",\"Val2\":25,\"Val3\":true}");
        }

        [Fact]
        public void WhenDehydrateMultiPropertyValue_ThenReturnsProperties()
        {
            var valueObject = new TestMultiValueObject("astringvalue", 25, true);
            var result = valueObject.Dehydrate();

            result.Should().Be("{\"Val1\":\"astringvalue\",\"Val2\":25,\"Val3\":true}");
        }

        [Fact]
        public void WhenDehydrateSingleListStringValue_ThenReturnsProperties()
        {
            var value = new List<string>
            {
                "avalue1",
                "avalue2"
            };
            var valueObject = new TestSingleListStringValueObject(value);
            var result = valueObject.Dehydrate();

            result.Should().Be("[\"avalue1\",\"avalue2\"]");
        }

        [Fact]
        public void WhenDehydrateSingleListValueObjectValueWithNullItems_ThenThrows()
        {
            var value = new List<TestSingleStringValueObject>
            {
                null,
                new TestSingleStringValueObject("avalue2")
            };
            var valueObject = new TestSingleListValueObjectValueObject(value);
            var result = valueObject.Dehydrate();

            result.Should().Be("[null,\"avalue2\"]");
        }

        [Fact]
        public void WhenDehydrateSingleListValueObjectValue_ThenReturnsProperties()
        {
            var value = new List<TestSingleStringValueObject>
            {
                new TestSingleStringValueObject("avalue1"),
                new TestSingleStringValueObject("avalue2")
            };
            var valueObject = new TestSingleListValueObjectValueObject(value);
            var result = valueObject.Dehydrate();

            result.Should().Be("[\"avalue1\",\"avalue2\"]");
        }

        [Fact]
        public void WhenRehydrateMultiValueWithNullValue_ThenReturnsInstance()
        {
            var valueObject = TestMultiValueObject.Rehydrate()("{\"Val1\":\"NULL\",\"Val2\":25,\"Val3\":True}", null);

            valueObject.AStringValue.Should().BeNull();
            valueObject.AnIntegerValue.Should().Be(25);
            valueObject.ABooleanValue.Should().BeTrue();
        }

        [Fact]
        public void WhenRehydrateMultiValue_ThenReturnsInstance()
        {
            var valueObject =
                TestMultiValueObject.Rehydrate()("{\"Val1\":\"astringvalue\",\"Val2\":25,\"Val3\":True}", null);

            valueObject.AStringValue.Should().Be("astringvalue");
            valueObject.AnIntegerValue.Should().Be(25);
            valueObject.ABooleanValue.Should().BeTrue();
        }

        [Fact]
        public void WhenEqualsWithNull_ThenReturnsFalse()
        {
            var result = new TestSingleStringValueObject("avalue").Equals(null);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEqualsWithDifferentValue_ThenReturnsFalse()
        {
            var result =
                new TestSingleStringValueObject("avalue").Equals(new TestSingleStringValueObject("anothervalue"));

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEqualsWithSameValue_ThenReturnsTrue()
        {
            var result = new TestSingleStringValueObject("avalue").Equals(new TestSingleStringValueObject("avalue"));

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenEqualsWithDifferentValueInMultiValueObject_ThenReturnsFalse()
        {
            var result =
                new TestMultiValueObject("avalue1", 25, true).Equals(new TestMultiValueObject("avalue2", 50, false));

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEqualsWithSameValueInMultiValueObject_ThenReturnsTrue()
        {
            var result =
                new TestMultiValueObject("avalue1", 25, true).Equals(new TestMultiValueObject("avalue1", 25, true));

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenEqualsWithNullStringValue_ThenReturnsFalse()
        {
            var result = new TestMultiValueObject("avalue", 25, true).Equals((string) null);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEqualsWithDifferentStringValue_ThenReturnsFalse()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var result = new TestMultiValueObject("avalue", 25, true).Equals("adifferentvalue");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEqualsWithSameStringValue_ThenReturnsTrue()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var result = new TestMultiValueObject("astringvalue", 25, true)
                .Equals("{\"Val1\":\"astringvalue\",\"Val2\":25,\"Val3\":true}");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenOperatorEqualsWithNullString_ThenReturnsFalse()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueObject == (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeFalse();
        }

        [Fact]
        public void WhenNotOperatorEqualsWithNullString_ThenReturnsTrue()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueObject != (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [Fact]
        public void WhenOperatorEqualsWithNullInstanceAndNullString_ThenReturnsTrue()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = (TestMultiValueObject) null == (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [Fact]
        public void WhenOperatorEqualsWithDifferentString_ThenReturnsFalse()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            var result = valueObject == "adifferentvalue";

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenOperatorEqualsWithSameString_ThenReturnsTrue()
        {
            var valueObject = new TestMultiValueObject("astringvalue", 25, true);

            var result = valueObject == "{\"Val1\":\"astringvalue\",\"Val2\":25,\"Val3\":true}";

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenOperatorEqualsWithNullValue_ThenReturnsFalse()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueObject == (TestMultiValueObject) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeFalse();
        }

        [Fact]
        public void WhenNotOperatorEqualsWithNullValue_ThenReturnsTrue()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueObject != (TestMultiValueObject) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [Fact]
        public void WhenOperatorEqualsWithNullInstanceAndNullValue_ThenReturnsTrue()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = null == (TestMultiValueObject) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [Fact]
        public void WhenOperatorEqualsWithDifferentValue_ThenReturnsFalse()
        {
            var result = new TestMultiValueObject("avalue1", 25, true) == new TestMultiValueObject("avalue2", 25, true);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenOperatorEqualsWithSameValue_ThenReturnsTrue()
        {
            // ReSharper disable once EqualExpressionComparison
            var result = new TestMultiValueObject("avalue1", 25, true) == new TestMultiValueObject("avalue1", 25, true);

            result.Should().BeTrue();
        }
    }

    public class TestSingleListStringValueObject : SingleValueObjectBase<TestSingleListStringValueObject,
        List<string>>
    {
        public TestSingleListStringValueObject(List<string> value) : base(value)
        {
        }

        public List<string> Values => Value;

        public static ValueObjectFactory<TestSingleListStringValueObject> Rehydrate()
        {
            return (property, container) => new TestSingleListStringValueObject(property.FromJson<List<string>>());
        }
    }

    public class TestSingleStringValueObject : SingleValueObjectBase<TestSingleStringValueObject, string>
    {
        public TestSingleStringValueObject(string value) : base(value)
        {
        }

        public string StringValue => Value;

        public static ValueObjectFactory<TestSingleStringValueObject> Rehydrate()
        {
            return (property, container) => new TestSingleStringValueObject(property);
        }
    }

    public class TestSingleEnumValueObject : SingleValueObjectBase<TestSingleEnumValueObject, TestEnum>
    {
        public TestSingleEnumValueObject(TestEnum value) : base(value)
        {
        }

        public TestEnum EnumValue => Value;

        public static ValueObjectFactory<TestSingleEnumValueObject> Rehydrate()
        {
            return (property, container) => new TestSingleEnumValueObject(property.ToEnumOrDefault(TestEnum.ADefault));
        }
    }

    public class TestSingleListValueObjectValueObject : SingleValueObjectBase<TestSingleListValueObjectValueObject,
        List<TestSingleStringValueObject>>
    {
        public TestSingleListValueObjectValueObject(List<TestSingleStringValueObject> value) : base(value)
        {
        }

        public List<TestSingleStringValueObject> Values => Value;

        public static ValueObjectFactory<TestSingleListValueObjectValueObject> Rehydrate()
        {
            return (property, container) => new TestSingleListValueObjectValueObject(property.FromJson<List<string>>()
                .Select(item => new TestSingleStringValueObject(item))
                .ToList());
        }
    }

    public class TestMultiValueObject : ValueObjectBase<TestMultiValueObject>
    {
        public TestMultiValueObject(string @string, int integer, bool boolean)
        {
            AStringValue = @string;
            AnIntegerValue = integer;
            ABooleanValue = boolean;
        }

        public string AStringValue { get; }

        public int AnIntegerValue { get; }

        public bool ABooleanValue { get; }

        public static ValueObjectFactory<TestMultiValueObject> Rehydrate()
        {
            return (property, container) =>
            {
                var values = RehydrateToList(property, false);
                return new TestMultiValueObject(values[0], values[1].ToInt(), values[2].ToBool());
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {AStringValue, AnIntegerValue, ABooleanValue};
        }
    }

    public enum TestEnum
    {
        ADefault = 0,
        AValue1 = 1,
        AValue2 = 2
    }

    [Trait("Category", "Unit")]
    public class ValueObjectExtensionsSpec
    {
        [Fact]
        public void WhenHasValueAndValueIsNull_ThenReturnsFalse()
        {
            var result = ((TestSingleStringValueObject) null).HasValue();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenHasValueAndValueIsNotNull_ThenReturnsTrue()
        {
            var result = new TestSingleStringValueObject("avalue").HasValue();

            result.Should().BeTrue();
        }
    }
}