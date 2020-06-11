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

            Assert.Equal("UnnamedTest", result.EntityCollection.Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithNamedEntityType_ThenCreatesANamedCollection()
        {
            var result = Query.From<NamedTestEntity>();

            Assert.Equal("aname", result.EntityCollection.Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithUnnamedUnconventionallyNamedType_ThenCreatesAFallbackNamedCollection()
        {
            var result = Query.From<UnnamedTestEntityUnconventionalNamed>();

            Assert.Equal("UnknownEntity", result.EntityCollection.Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmpty_ThenCreatesNoExpressions()
        {
            var result = Query.Empty<NamedTestEntity>();

            Assert.Equal("aname", result.EntityCollections[0].Name);
            Assert.Equal(0, result.EntityCollections[0].Expressions.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereOnEmpty_ThenThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Query.Empty<NamedTestEntity>().AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "1"));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWhereWithStringProperty_ThenCreatesAnExpression()
        {
            var result = Query.From<NamedTestEntity>().Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            Assert.Equal(1, result.EntityCollections[0].Expressions.Count);
            Assert.Equal(LogicalOperator.None, result.EntityCollections[0].Expressions[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[0].Condition.FieldName);
            Assert.Equal("1", result.EntityCollections[0].Expressions[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWhereWithDateTimeProperty_ThenCreatesAnExpression()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.ADateTimeProperty, ConditionOperator.EqualTo, datum);

            Assert.Equal(1, result.EntityCollections[0].Expressions.Count);
            Assert.Equal(LogicalOperator.None, result.EntityCollections[0].Expressions[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[0].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.EntityCollections[0].Expressions[0].Condition.FieldName);
            Assert.Equal(datum, result.EntityCollections[0].Expressions[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithStringProperty_ThenCreatesAnAndedExpression()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2");

            Assert.Equal(2, result.EntityCollections[0].Expressions.Count);
            Assert.Equal(LogicalOperator.None, result.EntityCollections[0].Expressions[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[0].Condition.FieldName);
            Assert.Equal("1", result.EntityCollections[0].Expressions[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.EntityCollections[0].Expressions[1].Operator);
            Assert.Equal(ConditionOperator.NotEqualTo, result.EntityCollections[0].Expressions[1].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[1].Condition.FieldName);
            Assert.Equal("2", result.EntityCollections[0].Expressions[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithDateTimeProperty_ThenCreatesAnAndedExpression()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(e => e.ADateTimeProperty, ConditionOperator.NotEqualTo, datum);

            Assert.Equal(2, result.EntityCollections[0].Expressions.Count);
            Assert.Equal(LogicalOperator.None, result.EntityCollections[0].Expressions[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[0].Condition.FieldName);
            Assert.Equal("1", result.EntityCollections[0].Expressions[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.EntityCollections[0].Expressions[1].Operator);
            Assert.Equal(ConditionOperator.NotEqualTo, result.EntityCollections[0].Expressions[1].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.EntityCollections[0].Expressions[1].Condition.FieldName);
            Assert.Equal(datum, result.EntityCollections[0].Expressions[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWhereWithStringProperty_ThenCreatesAnOredExpression()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .OrWhere(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2");

            Assert.Equal(2, result.EntityCollections[0].Expressions.Count);
            Assert.Equal(LogicalOperator.None, result.EntityCollections[0].Expressions[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[0].Condition.FieldName);
            Assert.Equal("1", result.EntityCollections[0].Expressions[0].Condition.Value);
            Assert.Equal(LogicalOperator.Or, result.EntityCollections[0].Expressions[1].Operator);
            Assert.Equal(ConditionOperator.NotEqualTo, result.EntityCollections[0].Expressions[1].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[1].Condition.FieldName);
            Assert.Equal("2", result.EntityCollections[0].Expressions[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWhereWithDateTimeProperty_ThenCreatesAnOredExpression()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .OrWhere(e => e.ADateTimeProperty, ConditionOperator.NotEqualTo, datum);

            Assert.Equal(2, result.EntityCollections[0].Expressions.Count);
            Assert.Equal(LogicalOperator.None, result.EntityCollections[0].Expressions[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[0].Condition.FieldName);
            Assert.Equal("1", result.EntityCollections[0].Expressions[0].Condition.Value);
            Assert.Equal(LogicalOperator.Or, result.EntityCollections[0].Expressions[1].Operator);
            Assert.Equal(ConditionOperator.NotEqualTo, result.EntityCollections[0].Expressions[1].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.EntityCollections[0].Expressions[1].Condition.FieldName);
            Assert.Equal(datum, result.EntityCollections[0].Expressions[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClause_ThenCreatesAnAndedNestedExpression()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(sub => sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2"));

            Assert.Equal(2, result.EntityCollections[0].Expressions.Count);
            Assert.Equal(LogicalOperator.None, result.EntityCollections[0].Expressions[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[0].Condition.FieldName);
            Assert.Equal("1", result.EntityCollections[0].Expressions[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.EntityCollections[0].Expressions[1].Operator);
            Assert.Equal(null, result.EntityCollections[0].Expressions[1].Condition);
            Assert.Equal(1, result.EntityCollections[0].Expressions[1].NestedExpressions.Count);
            Assert.Equal(ConditionOperator.NotEqualTo, result.EntityCollections[0].Expressions[1].NestedExpressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[1].NestedExpressions[0].Condition.FieldName);
            Assert.Equal("2", result.EntityCollections[0].Expressions[1].NestedExpressions[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClauses_ThenCreatesAnAndedNestedExpressions()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(sub =>
                    sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2")
                        .AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "3"));

            Assert.Equal(2, result.EntityCollections[0].Expressions.Count);
            Assert.Equal(LogicalOperator.None, result.EntityCollections[0].Expressions[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[0].Condition.FieldName);
            Assert.Equal("1", result.EntityCollections[0].Expressions[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.EntityCollections[0].Expressions[1].Operator);
            Assert.Equal(null, result.EntityCollections[0].Expressions[1].Condition);
            Assert.Equal(2, result.EntityCollections[0].Expressions[1].NestedExpressions.Count);
            Assert.Equal(ConditionOperator.NotEqualTo, result.EntityCollections[0].Expressions[1].NestedExpressions[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[1].NestedExpressions[0].Condition.FieldName);
            Assert.Equal("2", result.EntityCollections[0].Expressions[1].NestedExpressions[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.EntityCollections[0].Expressions[1].NestedExpressions[1].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.EntityCollections[0].Expressions[1].NestedExpressions[1].Condition.Operator);
            Assert.Equal("AStringProperty", result.EntityCollections[0].Expressions[1].NestedExpressions[1].Condition.FieldName);
            Assert.Equal("3", result.EntityCollections[0].Expressions[1].NestedExpressions[1].Condition.Value);
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
        public string AStringProperty => null;

        public DateTime ADateTimeProperty => default;

        public string EntityName => "aname";
    }
}