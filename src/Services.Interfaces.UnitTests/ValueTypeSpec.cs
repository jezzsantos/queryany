using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Interfaces.Entities;

namespace Services.Interfaces.UnitTests
{
    [TestClass]
    public class ValueTypeSpec
    {
        [TestMethod, TestCategory("Unit")]
        public void WhenEqualsWithNull_ThenReturnsFalse()
        {
            var result = new TestSingleValueType("avalue").Equals(null);

            Assert.IsFalse(result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEqualsWithDifferentValue_ThenReturnsFalse()
        {
            var result = new TestSingleValueType("avalue").Equals(new TestSingleValueType("anothervalue"));

            Assert.IsFalse(result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEqualsWithSameValue_ThenReturnsTrue()
        {
            var result = new TestSingleValueType("avalue").Equals(new TestSingleValueType("avalue"));

            Assert.IsTrue(result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEqualsWithDifferentValueInMultiValueType_ThenReturnsFalse()
        {
            var result =
                new TestMultiValueType("avalue1", 25, true).Equals(new TestMultiValueType("avalue2", 50, false));

            Assert.IsFalse(result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEqualsWithSameValueInMultiValueType_ThenReturnsTrue()
        {
            var result =
                new TestMultiValueType("avalue1", 25, true).Equals(new TestMultiValueType("avalue1", 25, true));

            Assert.IsTrue(result);
        }
    }

    public class TestSingleValueType : ValueType<TestSingleValueType>
    {
        private readonly string stringValue;

        public TestSingleValueType(string value)
        {
            this.stringValue = value;
        }

        public override string Dehydrate()
        {
            throw new NotImplementedException();
        }

        public override void Rehydrate(string value)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {this.stringValue};
        }
    }

    public class TestMultiValueType : ValueType<TestMultiValueType>
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


        public override string Dehydrate()
        {
            throw new NotImplementedException();
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
}