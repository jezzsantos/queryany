using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class ValueObjectSpec
    {
        [TestMethod]
        public void WhenDeserialized_ThenReturnsInstance()
        {
            var result = typeof(TestSingleStringValueObject).CreateInstance();

            result.Should().NotBeNull();
        }

        [TestMethod]
        public void WhenDehydrateSinglePropertyValue_ThenReturnsProperties()
        {
            var valueObject = new TestSingleStringValueObject("avalue");
            var result = valueObject.Dehydrate();

            result.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenDehydrateMultiPropertyValueWithNulls_ThenReturnsProperties()
        {
            var valueObject = new TestMultiValueObject(null, 25, true);
            var result = valueObject.Dehydrate();

            result.Should().Be("{\"Val1\":\"NULL\",\"Val2\":25,\"Val3\":true}");
        }

        [TestMethod]
        public void WhenDehydrateMultiPropertyValue_ThenReturnsProperties()
        {
            var valueObject = new TestMultiValueObject("astringvalue", 25, true);
            var result = valueObject.Dehydrate();

            result.Should().Be("{\"Val1\":\"astringvalue\",\"Val2\":25,\"Val3\":true}");
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void WhenRehydrateSingleValue_ThenReturnsInstance()
        {
            var valueObject = new TestSingleStringValueObject("avalue");
            valueObject.Rehydrate("anothervalue");

            valueObject.StringValue.Should().Be("anothervalue");
        }

        [TestMethod]
        public void WhenRehydrateSingleListStringValue_ThenReturnsInstance()
        {
            var valueObject = new TestSingleListStringValueObject(new List<string>());
            valueObject.Rehydrate("[\"avalue1\",\"avalue2\"]");

            valueObject.Values.Count.Should().Be(2);
            valueObject.Values[0].Should().Be("avalue1");
            valueObject.Values[1].Should().Be("avalue2");
        }

        [TestMethod]
        public void WhenRehydrateSingleListValueObjectValueWithNullValues_ThenReturnsInstance()
        {
            var valueObject = new TestSingleListValueObjectValueObject(new List<TestSingleStringValueObject>());
            valueObject.Rehydrate("[\"NULL\",\"avalue2\"]");

            valueObject.Values.Count.Should().Be(1);
            valueObject.Values[0].StringValue.Should().Be("avalue2");
        }

        [TestMethod]
        public void WhenRehydrateSingleListValueObjectValue_ThenReturnsInstance()
        {
            var valueObject = new TestSingleListValueObjectValueObject(new List<TestSingleStringValueObject>());
            valueObject.Rehydrate("[\"avalue1\",\"avalue2\"]");

            valueObject.Values.Count.Should().Be(2);
            valueObject.Values[0].StringValue.Should().Be("avalue1");
            valueObject.Values[1].StringValue.Should().Be("avalue2");
        }

        [TestMethod]
        public void WhenRehydrateMultiValueWithNullValue_ThenReturnsInstance()
        {
            var valueObject = new TestMultiValueObject("astringvalue", 25, true);
            valueObject.Rehydrate("{\"Val1\":\"NULL\",\"Val2\":25,\"Val3\":True}");

            valueObject.AStringValue.Should().BeNull();
            valueObject.AnIntegerValue.Should().Be(25);
            valueObject.ABooleanValue.Should().BeTrue();
        }

        [TestMethod]
        public void WhenRehydrateMultiValue_ThenReturnsInstance()
        {
            var valueObject = new TestMultiValueObject("astringvalue", 25, true);
            valueObject.Rehydrate("{\"Val1\":\"astringvalue\",\"Val2\":25,\"Val3\":True}");

            valueObject.AStringValue.Should().Be("astringvalue");
            valueObject.AnIntegerValue.Should().Be(25);
            valueObject.ABooleanValue.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEqualsWithNull_ThenReturnsFalse()
        {
            var result = new TestSingleStringValueObject("avalue").Equals(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithDifferentValue_ThenReturnsFalse()
        {
            var result =
                new TestSingleStringValueObject("avalue").Equals(new TestSingleStringValueObject("anothervalue"));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithSameValue_ThenReturnsTrue()
        {
            var result = new TestSingleStringValueObject("avalue").Equals(new TestSingleStringValueObject("avalue"));

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEqualsWithDifferentValueInMultiValueObject_ThenReturnsFalse()
        {
            var result =
                new TestMultiValueObject("avalue1", 25, true).Equals(new TestMultiValueObject("avalue2", 50, false));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithSameValueInMultiValueObject_ThenReturnsTrue()
        {
            var result =
                new TestMultiValueObject("avalue1", 25, true).Equals(new TestMultiValueObject("avalue1", 25, true));

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEqualsWithNullStringValue_ThenReturnsFalse()
        {
            var result = new TestMultiValueObject("avalue", 25, true).Equals((string) null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEqualsWithDifferentStringValue_ThenReturnsFalse()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var result = new TestMultiValueObject("avalue", 25, true).Equals("adifferentvalue");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEqualsWithSameStringValue_ThenReturnsTrue()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var result = new TestMultiValueObject("astringvalue", 25, true)
                .Equals("{\"Val1\":\"astringvalue\",\"Val2\":25,\"Val3\":true}");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithNullString_ThenReturnsFalse()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueObject == (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenNotOperatorEqualsWithNullString_ThenReturnsTrue()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueObject != (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithNullInstanceAndNullString_ThenReturnsTrue()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = (TestMultiValueObject) null == (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithDifferentString_ThenReturnsFalse()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            var result = valueObject == "adifferentvalue";

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithSameString_ThenReturnsTrue()
        {
            var valueObject = new TestMultiValueObject("astringvalue", 25, true);

            var result = valueObject == "{\"Val1\":\"astringvalue\",\"Val2\":25,\"Val3\":true}";

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithNullValue_ThenReturnsFalse()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueObject == (TestMultiValueObject) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenNotOperatorEqualsWithNullValue_ThenReturnsTrue()
        {
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueObject != (TestMultiValueObject) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithNullInstanceAndNullValue_ThenReturnsTrue()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = null == (TestMultiValueObject) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithDifferentValue_ThenReturnsFalse()
        {
            var result = new TestMultiValueObject("avalue1", 25, true) == new TestMultiValueObject("avalue2", 25, true);

            result.Should().BeFalse();
        }

        [TestMethod]
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

        protected override List<string> ToValue(string value)
        {
            return value.FromJson<List<string>>();
        }
    }

    public class TestSingleStringValueObject : SingleValueObjectBase<TestSingleStringValueObject, string>
    {
        public TestSingleStringValueObject(string value) : base(value)
        {
        }

        public string StringValue => Value;

        protected override string ToValue(string value)
        {
            return value;
        }
    }

    public class TestSingleListValueObjectValueObject : SingleValueObjectBase<TestSingleListValueObjectValueObject,
        List<TestSingleStringValueObject>>
    {
        public TestSingleListValueObjectValueObject(List<TestSingleStringValueObject> value) : base(value)
        {
        }

        public List<TestSingleStringValueObject> Values => Value;

        protected override List<TestSingleStringValueObject> ToValue(string value)
        {
            return value.FromJson<List<string>>()
                .Select(item => new TestSingleStringValueObject(item))
                .ToList();
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

        public string AStringValue { get; private set; }

        public int AnIntegerValue { get; private set; }

        public bool ABooleanValue { get; private set; }

        public override void Rehydrate(string value)
        {
            var values = RehydrateToList(value);
            AStringValue = values[0];
            AnIntegerValue = values[1].ToInt();
            ABooleanValue = values[2].ToBool();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {AStringValue, AnIntegerValue, ABooleanValue};
        }
    }

    [TestClass, TestCategory("Unit")]
    public class ValueObjectExtensionsSpec
    {
        [TestMethod]
        public void WhenHasValueAndValueIsNull_ThenReturnsFalse()
        {
            var result = ((TestSingleStringValueObject) null).HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndValueIsNotNull_ThenReturnsTrue()
        {
            var result = new TestSingleStringValueObject("avalue").HasValue();

            result.Should().BeTrue();
        }
    }
}