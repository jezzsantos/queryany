using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class ValueTypeSpec
    {
        [TestMethod]
        public void WhenDeserialized_ThenReturnsInstance()
        {
            var result = typeof(TestSingleValueType).CreateInstance();

            result.Should().NotBeNull();
        }

        [TestMethod]
        public void WhenHydrate_ThenReturnsProperties()
        {
            var valueType = new TestSingleValueType("avalue");
            var result = valueType.Dehydrate();

            result.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenDehydrate_ThenReturnsInstance()
        {
            var valueType = new TestSingleValueType(null);
            valueType.Rehydrate("avalue");

            valueType.StringValue.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenEqualsWithNull_ThenReturnsFalse()
        {
            var result = new TestSingleValueType("avalue").Equals((TestSingleValueType) null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithDifferentValue_ThenReturnsFalse()
        {
            var result = new TestSingleValueType("avalue").Equals(new TestSingleValueType("anothervalue"));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithSameValue_ThenReturnsTrue()
        {
            var result = new TestSingleValueType("avalue").Equals(new TestSingleValueType("avalue"));

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEqualsWithDifferentValueInMultiValueType_ThenReturnsFalse()
        {
            var result =
                new TestMultiValueType("avalue1", 25, true).Equals(new TestMultiValueType("avalue2", 50, false));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithSameValueInMultiValueType_ThenReturnsTrue()
        {
            var result =
                new TestMultiValueType("avalue1", 25, true).Equals(new TestMultiValueType("avalue1", 25, true));

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEqualsWithNullStringValue_ThenReturnsFalse()
        {
            var result = new TestMultiValueType("avalue", 25, true).Equals((string) null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEqualsWithDifferentStringValue_ThenReturnsFalse()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var result = new TestMultiValueType("avalue", 25, true).Equals("adifferentvalue");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEqualsWithSameStringValue_ThenReturnsTrue()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var result = new TestMultiValueType("avalue", 25, true).Equals("avalue::25::True");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithNullString_ThenReturnsFalse()
        {
            var valueType = new TestMultiValueType("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueType == (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenNotOperatorEqualsWithNullString_ThenReturnsTrue()
        {
            var valueType = new TestMultiValueType("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueType != (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithNullInstanceAndNullString_ThenReturnsTrue()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = (TestMultiValueType) null == (string) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithDifferentString_ThenReturnsFalse()
        {
            var valueType = new TestMultiValueType("avalue", 25, true);

            var result = valueType == "adifferentvalue";

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithSameString_ThenReturnsTrue()
        {
            var valueType = new TestMultiValueType("avalue", 25, true);

            var result = valueType == "avalue::25::True";

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithNullValue_ThenReturnsFalse()
        {
            var valueType = new TestMultiValueType("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueType == (TestMultiValueType) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenNotOperatorEqualsWithNullValue_ThenReturnsTrue()
        {
            var valueType = new TestMultiValueType("avalue", 25, true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = valueType != (TestMultiValueType) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithNullInstanceAndNullValue_ThenReturnsTrue()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var result = null == (TestMultiValueType) null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithDifferentValue_ThenReturnsFalse()
        {
            var result = new TestMultiValueType("avalue1", 25, true) == new TestMultiValueType("avalue2", 25, true);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenOperatorEqualsWithSameValue_ThenReturnsTrue()
        {
            // ReSharper disable once EqualExpressionComparison
            var result = new TestMultiValueType("avalue1", 25, true) == new TestMultiValueType("avalue1", 25, true);

            result.Should().BeTrue();
        }
    }

    public class TestSingleValueType : ValueTypeBase<TestSingleValueType>
    {
        public TestSingleValueType(string value)
        {
            StringValue = value;
        }

        public string StringValue { get; private set; }

        public override string Dehydrate()
        {
            return StringValue;
        }

        public override void Rehydrate(string value)
        {
            StringValue = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {StringValue};
        }
    }

    public class TestMultiValueType : ValueTypeBase<TestMultiValueType>
    {
        private readonly bool boolean;
        private readonly int integer;
        private readonly string @string;

        public TestMultiValueType(string @string, int integer, bool boolean)
        {
            this.@string = @string;
            this.integer = integer;
            this.boolean = boolean;
        }

        public override void Rehydrate(string value)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {this.@string, this.integer, this.boolean};
        }
    }

    [TestClass, TestCategory("Unit")]
    public class ValueTypeExtensionsSpec
    {
        [TestMethod]
        public void WhenHasValueAndValueIsNull_ThenReturnsFalse()
        {
            var result = ((TestSingleValueType) null).HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndValueIsNotNull_ThenReturnsTrue()
        {
            var result = new TestSingleValueType("avalue").HasValue();

            result.Should().BeTrue();
        }
    }
}