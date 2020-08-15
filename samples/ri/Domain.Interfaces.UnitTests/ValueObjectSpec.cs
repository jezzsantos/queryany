﻿using System;
using System.Collections.Generic;
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
            var result = typeof(TestSingleValueObject).CreateInstance();

            result.Should().NotBeNull();
        }

        [TestMethod]
        public void WhenHydrate_ThenReturnsProperties()
        {
            var valueObject = new TestSingleValueObject("avalue");
            var result = valueObject.Dehydrate();

            result.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenDehydrate_ThenReturnsInstance()
        {
            var valueObject = new TestSingleValueObject(null);
            valueObject.Rehydrate("avalue");

            valueObject.StringValue.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenEqualsWithNull_ThenReturnsFalse()
        {
            var result = new TestSingleValueObject("avalue").Equals((TestSingleValueObject) null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithDifferentValue_ThenReturnsFalse()
        {
            var result = new TestSingleValueObject("avalue").Equals(new TestSingleValueObject("anothervalue"));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithSameValue_ThenReturnsTrue()
        {
            var result = new TestSingleValueObject("avalue").Equals(new TestSingleValueObject("avalue"));

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEqualsWithDifferentValueInMultivalueObject_ThenReturnsFalse()
        {
            var result =
                new TestMultiValueObject("avalue1", 25, true).Equals(new TestMultiValueObject("avalue2", 50, false));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenEqualsWithSameValueInMultivalueObject_ThenReturnsTrue()
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
            var result = new TestMultiValueObject("avalue", 25, true).Equals("avalue::25::True");

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
            var valueObject = new TestMultiValueObject("avalue", 25, true);

            var result = valueObject == "avalue::25::True";

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

    public class TestSingleValueObject : ValueObjectBase<TestSingleValueObject>
    {
        public TestSingleValueObject(string value)
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

    public class TestMultiValueObject : ValueObjectBase<TestMultiValueObject>
    {
        private readonly bool boolean;
        private readonly int integer;
        private readonly string @string;

        public TestMultiValueObject(string @string, int integer, bool boolean)
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
    public class ValueObjectExtensionsSpec
    {
        [TestMethod]
        public void WhenHasValueAndValueIsNull_ThenReturnsFalse()
        {
            var result = ((TestSingleValueObject) null).HasValue();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenHasValueAndValueIsNotNull_ThenReturnsTrue()
        {
            var result = new TestSingleValueObject("avalue").HasValue();

            result.Should().BeTrue();
        }
    }
}