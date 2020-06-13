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

            Assert.Equal("UnnamedTest", result.Entities[0].Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithNamedEntityType_ThenCreatesANamedCollection()
        {
            var result = Query.From<NamedTestEntity>();

            Assert.Equal("aname", result.Entities[0].Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithUnnamedUnconventionallyNamedType_ThenCreatesAFallbackNamedCollection()
        {
            var result = Query.From<UnnamedTestEntityUnconventionalNamed>();

            Assert.Equal("UnknownEntity", result.Entities[0].Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmpty_ThenCreatesNoWheres()
        {
            var result = Query.Empty<NamedTestEntity>();

            Assert.Equal("aname", result.Entities[0].Name);
            Assert.Equal(0, result.Wheres.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereOnEmpty_ThenThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Query.Empty<NamedTestEntity>().AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "1"));
        }


        [TestMethod, TestCategory("Unit")]
        public void WhenWhereAll_ThenCreatesAWhere()
        {
            var result = Query.From<NamedTestEntity>().WhereAll();

            Assert.Equal(0, result.Wheres.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereAfterWhereAll_ThenThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Query.From<NamedTestEntity>()
                    .WhereAll()
                    .AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "1"));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWhereWithStringProperty_ThenCreatesAWhere()
        {
            var result = Query.From<NamedTestEntity>().Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            Assert.Equal(1, result.Wheres.Count);
            Assert.Equal(LogicalOperator.None, result.Wheres[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.Wheres[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[0].Condition.FieldName);
            Assert.Equal("1", result.Wheres[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWhereWithDateTimeProperty_ThenCreatesAWhere()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.ADateTimeProperty, ConditionOperator.EqualTo, datum);

            Assert.Equal(1, result.Wheres.Count);
            Assert.Equal(LogicalOperator.None, result.Wheres[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.Wheres[0].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.Wheres[0].Condition.FieldName);
            Assert.Equal(datum, result.Wheres[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithStringProperty_ThenCreatesAnAndedWhere()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2");

            Assert.Equal(2, result.Wheres.Count);
            Assert.Equal(LogicalOperator.None, result.Wheres[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.Wheres[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[0].Condition.FieldName);
            Assert.Equal("1", result.Wheres[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.Wheres[1].Operator);
            Assert.Equal(ConditionOperator.NotEqualTo,
                result.Wheres[1].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[1].Condition.FieldName);
            Assert.Equal("2", result.Wheres[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithDateTimeProperty_ThenCreatesAnAndedWhere()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(e => e.ADateTimeProperty, ConditionOperator.NotEqualTo, datum);

            Assert.Equal(2, result.Wheres.Count);
            Assert.Equal(LogicalOperator.None, result.Wheres[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.Wheres[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[0].Condition.FieldName);
            Assert.Equal("1", result.Wheres[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.Wheres[1].Operator);
            Assert.Equal(ConditionOperator.NotEqualTo,
                result.Wheres[1].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.Wheres[1].Condition.FieldName);
            Assert.Equal(datum, result.Wheres[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWhereWithStringProperty_ThenCreatesAnOredWhere()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .OrWhere(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2");

            Assert.Equal(2, result.Wheres.Count);
            Assert.Equal(LogicalOperator.None, result.Wheres[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.Wheres[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[0].Condition.FieldName);
            Assert.Equal("1", result.Wheres[0].Condition.Value);
            Assert.Equal(LogicalOperator.Or, result.Wheres[1].Operator);
            Assert.Equal(ConditionOperator.NotEqualTo,
                result.Wheres[1].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[1].Condition.FieldName);
            Assert.Equal("2", result.Wheres[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWhereWithDateTimeProperty_ThenCreatesAnOredWhere()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .OrWhere(e => e.ADateTimeProperty, ConditionOperator.NotEqualTo, datum);

            Assert.Equal(2, result.Wheres.Count);
            Assert.Equal(LogicalOperator.None, result.Wheres[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.Wheres[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[0].Condition.FieldName);
            Assert.Equal("1", result.Wheres[0].Condition.Value);
            Assert.Equal(LogicalOperator.Or, result.Wheres[1].Operator);
            Assert.Equal(ConditionOperator.NotEqualTo,
                result.Wheres[1].Condition.Operator);
            Assert.Equal("ADateTimeProperty", result.Wheres[1].Condition.FieldName);
            Assert.Equal(datum, result.Wheres[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClause_ThenCreatesAnAndedNestedWhere()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(sub => sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2"));

            Assert.Equal(2, result.Wheres.Count);
            Assert.Equal(LogicalOperator.None, result.Wheres[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.Wheres[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[0].Condition.FieldName);
            Assert.Equal("1", result.Wheres[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.Wheres[1].Operator);
            Assert.Equal(null, result.Wheres[1].Condition);
            Assert.Equal(1, result.Wheres[1].NestedWheres.Count);
            Assert.Equal(ConditionOperator.NotEqualTo,
                result.Wheres[1].NestedWheres[0].Condition.Operator);
            Assert.Equal("AStringProperty",
                result.Wheres[1].NestedWheres[0].Condition.FieldName);
            Assert.Equal("2", result.Wheres[1].NestedWheres[0].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClauses_ThenCreatesAnAndedNestedWheres()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(sub =>
                    sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2")
                        .AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "3"));

            Assert.Equal(2, result.Wheres.Count);
            Assert.Equal(LogicalOperator.None, result.Wheres[0].Operator);
            Assert.Equal(ConditionOperator.EqualTo, result.Wheres[0].Condition.Operator);
            Assert.Equal("AStringProperty", result.Wheres[0].Condition.FieldName);
            Assert.Equal("1", result.Wheres[0].Condition.Value);
            Assert.Equal(LogicalOperator.And, result.Wheres[1].Operator);
            Assert.Equal(null, result.Wheres[1].Condition);
            Assert.Equal(2, result.Wheres[1].NestedWheres.Count);
            Assert.Equal(ConditionOperator.NotEqualTo,
                result.Wheres[1].NestedWheres[0].Condition.Operator);
            Assert.Equal("AStringProperty",
                result.Wheres[1].NestedWheres[0].Condition.FieldName);
            Assert.Equal("2", result.Wheres[1].NestedWheres[0].Condition.Value);
            Assert.Equal(LogicalOperator.And,
                result.Wheres[1].NestedWheres[1].Operator);
            Assert.Equal(ConditionOperator.EqualTo,
                result.Wheres[1].NestedWheres[1].Condition.Operator);
            Assert.Equal("AStringProperty",
                result.Wheres[1].NestedWheres[1].Condition.FieldName);
            Assert.Equal("3", result.Wheres[1].NestedWheres[1].Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenJoin_ThenCreatesAnInnerJoin()
        {
            var result = Query.From<FirstTestEntity>()
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty)
                .Where(e => e.AFirstStringProperty, ConditionOperator.EqualTo, "avalue");

            Assert.Equal(2, result.Entities.Count);
            Assert.Null(result.Entities[0].Join);
            Assert.Equal("first", result.Entities[1].Join.Left.EntityName);
            Assert.Equal("AFirstStringProperty", result.Entities[1].Join.Left.JoinedFieldName);
            Assert.Equal("second", result.Entities[1].Join.Right.EntityName);
            Assert.Equal("ASecondStringProperty", result.Entities[1].Join.Right.JoinedFieldName);
            Assert.Equal(JoinType.Inner, result.Entities[1].Join.Type);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenJoinMultipleEntities_ThenCreatesJoins()
        {
            var result = Query.From<FirstTestEntity>()
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty,
                    JoinType.Outer)
                .AndJoin<ThirdTestEntity, DateTime>(f => f.AFirstDateTimeProperty, t => t.AThirdDateTimeProperty)
                .Where(e => e.AFirstStringProperty, ConditionOperator.EqualTo, "avalue");

            Assert.Equal(3, result.Entities.Count);
            Assert.Null(result.Entities[0].Join);
            Assert.Equal("first", result.Entities[1].Join.Left.EntityName);
            Assert.Equal("AFirstStringProperty", result.Entities[1].Join.Left.JoinedFieldName);
            Assert.Equal("second", result.Entities[1].Join.Right.EntityName);
            Assert.Equal("ASecondStringProperty", result.Entities[1].Join.Right.JoinedFieldName);
            Assert.Equal(JoinType.Outer, result.Entities[1].Join.Type);

            Assert.Equal("first", result.Entities[2].Join.Left.EntityName);
            Assert.Equal("AFirstDateTimeProperty", result.Entities[2].Join.Left.JoinedFieldName);
            Assert.Equal("third", result.Entities[2].Join.Right.EntityName);
            Assert.Equal("AThirdDateTimeProperty", result.Entities[2].Join.Right.JoinedFieldName);
            Assert.Equal(JoinType.Inner, result.Entities[2].Join.Type);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenJoinWithMultipleJoinsWithSameType_ThenThrows()
        {
            Assert.Throws<InvalidOperationException>(() => Query.From<FirstTestEntity>()
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty)
                .AndJoin<SecondTestEntity, DateTime>(f => f.AFirstDateTimeProperty, t => t.ASecondDateTimeProperty));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenJoinWithMultipleJoinsOnSameProperty_ThenCreatesJoins()
        {
            var result = Query.From<FirstTestEntity>()
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty,
                    JoinType.Outer)
                .AndJoin<ThirdTestEntity, string>(f => f.AFirstStringProperty, t => t.AThirdStringProperty)
                .Where(e => e.AFirstStringProperty, ConditionOperator.EqualTo, "avalue");

            Assert.Equal(3, result.Entities.Count);
            Assert.Null(result.Entities[0].Join);
            Assert.Equal("first", result.Entities[1].Join.Left.EntityName);
            Assert.Equal("AFirstStringProperty", result.Entities[1].Join.Left.JoinedFieldName);
            Assert.Equal("second", result.Entities[1].Join.Right.EntityName);
            Assert.Equal("ASecondStringProperty", result.Entities[1].Join.Right.JoinedFieldName);
            Assert.Equal(JoinType.Outer, result.Entities[1].Join.Type);

            Assert.Equal("first", result.Entities[2].Join.Left.EntityName);
            Assert.Equal("AFirstStringProperty", result.Entities[2].Join.Left.JoinedFieldName);
            Assert.Equal("third", result.Entities[2].Join.Right.EntityName);
            Assert.Equal("AThirdStringProperty", result.Entities[2].Join.Right.JoinedFieldName);
            Assert.Equal(JoinType.Inner, result.Entities[2].Join.Type);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenNoSelect_ThenSelectsAllFields()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue");

            Assert.Equal(0, result.Entities[0].Selects.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSelectWithFromEntityField_ThenFieldSelected()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue")
                .Select(e => e.ADateTimeProperty);

            Assert.Equal(1, result.Entities[0].Selects.Count);
            Assert.Equal("ADateTimeProperty", result.Entities[0].Selects[0].Name);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSelectWithFromEntityFields_ThenFieldsSelected()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue")
                .Select(e => e.AStringProperty).Select(e => e.ADateTimeProperty);

            Assert.Equal(2, result.Entities[0].Selects.Count);
            Assert.Equal("AStringProperty", result.Entities[0].Selects[0].Name);
            Assert.Equal("ADateTimeProperty", result.Entities[0].Selects[1].Name);
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

    public class FirstTestEntity : INamedEntity
    {
        public string AFirstStringProperty => null;
        public DateTime AFirstDateTimeProperty => DateTime.MinValue;
        public string EntityName => "first";
    }

    public class SecondTestEntity : INamedEntity
    {
        public string ASecondStringProperty => null;
        public DateTime ASecondDateTimeProperty => DateTime.MinValue;

        public string EntityName => "second";
    }

    public class ThirdTestEntity : INamedEntity
    {
        public string AThirdStringProperty => null;
        public DateTime AThirdDateTimeProperty => DateTime.MinValue;

        public string EntityName => "third";
    }
}