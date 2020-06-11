﻿using System;
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
        public void WhenEmpty_ThenCreatesNoExpressions()
        {
            var result = Query.Empty<NamedTestEntity>();

            Assert.Equal("aname", result.Collections[0].Name);
            Assert.Equal(0, result.Collections[0].Expressions.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereOnEmpty_ThenThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Query.Empty<NamedTestEntity>().AndWhere(e => e.AStringProperty, Condition.Eq, "1"));
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

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClause_ThenCreatesAnAndedNestedExpression()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, Condition.Eq, "1")
                .AndWhere(sub => sub.Where(e => e.AStringProperty, Condition.Ne, "2"));

            Assert.Equal(2, result.Collections[0].Expressions.Count);
            Assert.Equal(Combine.None, result.Collections[0].Expressions[0].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[0].Condition.Column);
            Assert.Equal("1", result.Collections[0].Expressions[0].Condition.Value);
            Assert.Equal(Combine.And, result.Collections[0].Expressions[1].Combiner);
            Assert.Equal(null, result.Collections[0].Expressions[1].Condition);
            Assert.Equal(1, result.Collections[0].Expressions[1].NestedExpressions.Count);
            Assert.Equal(Condition.Ne, result.Collections[0].Expressions[1].NestedExpressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[1].NestedExpressions[0].Condition.Column);
            Assert.Equal("2", result.Collections[0].Expressions[1].NestedExpressions[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClauses_ThenCreatesAnAndedNestedExpressions()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, Condition.Eq, "1")
                .AndWhere(sub =>
                    sub.Where(e => e.AStringProperty, Condition.Ne, "2")
                        .AndWhere(e => e.AStringProperty, Condition.Eq, "3"));

            Assert.Equal(2, result.Collections[0].Expressions.Count);
            Assert.Equal(Combine.None, result.Collections[0].Expressions[0].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[0].Condition.Column);
            Assert.Equal("1", result.Collections[0].Expressions[0].Condition.Value);
            Assert.Equal(Combine.And, result.Collections[0].Expressions[1].Combiner);
            Assert.Equal(null, result.Collections[0].Expressions[1].Condition);
            Assert.Equal(2, result.Collections[0].Expressions[1].NestedExpressions.Count);
            Assert.Equal(Condition.Ne, result.Collections[0].Expressions[1].NestedExpressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[1].NestedExpressions[0].Condition.Column);
            Assert.Equal("2", result.Collections[0].Expressions[1].NestedExpressions[0].Condition.Value);
            Assert.Equal(Combine.And, result.Collections[0].Expressions[1].NestedExpressions[1].Combiner);
            Assert.Equal(Condition.Eq, result.Collections[0].Expressions[1].NestedExpressions[1].Condition.Operator);
            Assert.Equal("AStringProperty", result.Collections[0].Expressions[1].NestedExpressions[1].Condition.Column);
            Assert.Equal("3", result.Collections[0].Expressions[1].NestedExpressions[1].Condition.Value);
        }
    }

    public class UnnamedTestEntity : INamedEntity
    {
        public string EntityName => null;
    }

    public class UnnamedTestEntityUnconventionalNamed : INamedEntity
    {
        public string EntityName => null;
    }

    public class NamedTestEntity : INamedEntity
    {
        public string AStringProperty { get; set; }
        public DateTime ADateTimeProperty { get; set; }
        public string EntityName => "aname";
    }
}