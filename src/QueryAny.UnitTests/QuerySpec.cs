using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueryAny.UnitTests
{
    [TestClass]
    public class QuerySpec
    {
        private static readonly IAssertion Assert = new Assertion();

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithUnnamedEntityType_ThenCreatesADerivedNamedCollection()
        {
            var result = Query.From<UnnamedTestEntity>();

            Assert.Equal("UnnamedTest", result.Collection.Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithNamedEntityType_ThenCreatesANamedCollection()
        {
            var result = Query.From<NamedTestEntity>();

            Assert.Equal("aname", result.Collection.Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithUnnamedUnconventionallyNamedType_ThenCreatesAFallbackNamedCollection()
        {
            var result = Query.From<UnnamedTestEntityUnconventionalNamed>();

            Assert.Equal("UnknownEntity", result.Collection.Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWhereWithStringProperty_ThenCreatesAnExpression()
        {
            var result = Query.From<NamedTestEntity>().Where(e => e.AStringProperty, Condition.Eq, "1");

            Assert.Equal(1, result.Collections[0].Expressions.Count);
            Assert.Equal(Combine.None, result.Collections[0].Expressions[0].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[0].Condition.Column);
            Assert.Equal("1", result.Collections[0].Expressions[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWhereWithDateTimeProperty_ThenCreatesAnExpression()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.ADateTimeProperty, Condition.Eq, datum);

            Assert.Equal(1, result.Collections[0].Expressions.Count);
            Assert.Equal(Combine.None, result.Collections[0].Expressions[0].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[0].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.Collections[0].Expressions[0].Condition.Column);
            Assert.Equal(datum, result.Collections[0].Expressions[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithStringProperty_ThenCreatesAnAndedExpression()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, Condition.Eq, "1")
                .AndWhere(e => e.AStringProperty, Condition.Ne, "2");

            Assert.Equal(2, result.Collections[0].Expressions.Count);
            Assert.Equal(Combine.None, result.Collections[0].Expressions[0].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[0].Condition.Column);
            Assert.Equal("1", result.Collections[0].Expressions[0].Condition.Value);
            Assert.Equal(Combine.And, result.Collections[0].Expressions[1].Combiner);
            Assert.Equal(Condition.Ne, result.Collections[0].Expressions[1].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[1].Condition.Column);
            Assert.Equal("2", result.Collections[0].Expressions[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithDateTimeProperty_ThenCreatesAnAndedExpression()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, Condition.Eq, "1")
                .AndWhere(e => e.ADateTimeProperty, Condition.Ne, datum);

            Assert.Equal(2, result.Collections[0].Expressions.Count);
            Assert.Equal(Combine.None, result.Collections[0].Expressions[0].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[0].Condition.Column);
            Assert.Equal("1", result.Collections[0].Expressions[0].Condition.Value);
            Assert.Equal(Combine.And, result.Collections[0].Expressions[1].Combiner);
            Assert.Equal(Condition.Ne, result.Collections[0].Expressions[1].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.Collections[0].Expressions[1].Condition.Column);
            Assert.Equal(datum, result.Collections[0].Expressions[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWhereWithStringProperty_ThenCreatesAnOredExpression()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, Condition.Eq, "1")
                .OrWhere(e => e.AStringProperty, Condition.Ne, "2");

            Assert.Equal(2, result.Collections[0].Expressions.Count);
            Assert.Equal(Combine.None, result.Collections[0].Expressions[0].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[0].Condition.Column);
            Assert.Equal("1", result.Collections[0].Expressions[0].Condition.Value);
            Assert.Equal(Combine.Or, result.Collections[0].Expressions[1].Combiner);
            Assert.Equal(Condition.Ne, result.Collections[0].Expressions[1].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[1].Condition.Column);
            Assert.Equal("2", result.Collections[0].Expressions[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWhereWithDateTimeProperty_ThenCreatesAnOredExpression()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, Condition.Eq, "1")
                .OrWhere(e => e.ADateTimeProperty, Condition.Ne, datum);

            Assert.Equal(2, result.Collections[0].Expressions.Count);
            Assert.Equal(Combine.None, result.Collections[0].Expressions[0].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[0].Condition.Column);
            Assert.Equal("1", result.Collections[0].Expressions[0].Condition.Value);
            Assert.Equal(Combine.Or, result.Collections[0].Expressions[1].Combiner);
            Assert.Equal(Condition.Ne, result.Collections[0].Expressions[1].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.Collections[0].Expressions[1].Condition.Column);
            Assert.Equal(datum, result.Collections[0].Expressions[1].Condition.Value);
        }
    }

    public class UnnamedTestEntity : INamedEntity
    {
        public string Name => null;
    }

    public class UnnamedTestEntityUnconventionalNamed : INamedEntity
    {
        public string Name => null;
    }

    public class NamedTestEntity : INamedEntity
    {
        public string AStringProperty { get; set; }
        public DateTime ADateTimeProperty { get; set; }
        public string Name => "aname";
    }
}