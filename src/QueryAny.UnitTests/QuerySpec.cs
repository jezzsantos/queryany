using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Properties;

namespace QueryAny.UnitTests
{
    [TestClass]
    public class QuerySpec
    {
        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithUnnamedEntityType_ThenCreatesADerivedNamedCollection()
        {
            var result = Query.From<UnnamedTestEntity>();

            result.PrimaryEntity.EntityName.Should().Be("UnnamedTest");
            result.PrimaryEntity.EntityName.Should().Be("UnnamedTest");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithNamedEntityType_ThenCreatesANamedCollection()
        {
            var result = Query.From<NamedTestEntity>();

            result.PrimaryEntity.EntityName.Should().Be("aname");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithUnnamedUnconventionallyNamedType_ThenCreatesDefaultNamedCollection()
        {
            var result = Query.From<UnnamedTestEntityUnconventionalNamed>();

            result.PrimaryEntity.EntityName.Should().Be("unnamedtestentityunconventionalnamed");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmpty_ThenCreatesNoWheres()
        {
            var result = Query.Empty<NamedTestEntity>();

            result.PrimaryEntity.EntityName.Should().Be("aname");
            result.Wheres.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereOnEmpty_ThenThrows()
        {
            Query.Empty<NamedTestEntity>()
                .Invoking(x => x.AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "1"))
                .Should().Throw<InvalidOperationException>();
        }


        [TestMethod, TestCategory("Unit")]
        public void WhenWhereAll_ThenCreatesAWhere()
        {
            var result = Query.From<NamedTestEntity>().WhereAll();

            result.Wheres.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereAfterWhereAll_ThenThrows()
        {
            Query.From<NamedTestEntity>()
                .WhereAll().Invoking(x => x.AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "1"))
                .Should().Throw<InvalidOperationException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWhereWithStringProperty_ThenCreatesAWhere()
        {
            var result = Query.From<NamedTestEntity>().Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("1");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWhereWithDateTimeProperty_ThenCreatesAWhere()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.ADateTimeProperty, ConditionOperator.EqualTo, datum);

            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("ADateTimeProperty");
            result.Wheres[0].Condition.Value.Should().Be(datum);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithStringProperty_ThenCreatesAnAndedWhere()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2");

            result.Wheres.Count.Should().Be(2);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("1");
            result.Wheres[1].Operator.Should().Be(LogicalOperator.And);
            result.Wheres[1].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[1].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[1].Condition.Value.Should().Be("2");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithDateTimeProperty_ThenCreatesAnAndedWhere()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(e => e.ADateTimeProperty, ConditionOperator.NotEqualTo, datum);

            result.Wheres.Count.Should().Be(2);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("1");
            result.Wheres[1].Operator.Should().Be(LogicalOperator.And);
            result.Wheres[1].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[1].Condition.FieldName.Should().Be("ADateTimeProperty");
            result.Wheres[1].Condition.Value.Should().Be(datum);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWhereWithStringProperty_ThenCreatesAnOredWhere()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .OrWhere(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2");

            result.Wheres.Count.Should().Be(2);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("1");
            result.Wheres[1].Operator.Should().Be(LogicalOperator.Or);
            result.Wheres[1].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[1].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[1].Condition.Value.Should().Be("2");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWhereWithDateTimeProperty_ThenCreatesAnOredWhere()
        {
            var datum = DateTime.UtcNow;
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .OrWhere(e => e.ADateTimeProperty, ConditionOperator.NotEqualTo, datum);

            result.Wheres.Count.Should().Be(2);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("1");
            result.Wheres[1].Operator.Should().Be(LogicalOperator.Or);
            result.Wheres[1].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[1].Condition.FieldName.Should().Be("ADateTimeProperty");
            result.Wheres[1].Condition.Value.Should().Be(datum);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClause_ThenCreatesAnAndedNestedWhere()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(sub => sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2"));

            result.Wheres.Count.Should().Be(2);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("1");
            result.Wheres[1].Operator.Should().Be(LogicalOperator.And);
            result.Wheres[1].Condition.Should().Be(null);
            result.Wheres[1].NestedWheres.Count.Should().Be(1);
            result.Wheres[1].NestedWheres[0].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[1].NestedWheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[1].NestedWheres[0].Condition.Value.Should().Be("2");
        }

        
        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClause_ThenCreatesAnOredNestedWhere()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .OrWhere(sub => sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2"));

            result.Wheres.Count.Should().Be(2);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("1");
            result.Wheres[1].Operator.Should().Be(LogicalOperator.Or);
            result.Wheres[1].Condition.Should().Be(null);
            result.Wheres[1].NestedWheres.Count.Should().Be(1);
            result.Wheres[1].NestedWheres[0].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[1].NestedWheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[1].NestedWheres[0].Condition.Value.Should().Be("2");
        }
        
        [TestMethod, TestCategory("Unit")]
        public void WhenAndWhereWithSubWhereClauses_ThenCreatesAnAndedNestedWheres()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1")
                .AndWhere(sub =>
                    sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2")
                        .AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "3"));

            result.Wheres.Count.Should().Be(2);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("1");
            result.Wheres[1].Operator.Should().Be(LogicalOperator.And);
            result.Wheres[1].Condition.Should().Be(null);
            result.Wheres[1].NestedWheres.Count.Should().Be(2);
            result.Wheres[1].NestedWheres[0].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[1].NestedWheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[1].NestedWheres[0].Condition.Value.Should().Be("2");
            result.Wheres[1].NestedWheres[1].Operator.Should().Be(LogicalOperator.And);
            result.Wheres[1].NestedWheres[1].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[1].NestedWheres[1].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[1].NestedWheres[1].Condition.Value.Should().Be("3");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenJoin_ThenCreatesAnInnerJoin()
        {
            var result = Query.From<FirstTestEntity>()
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty)
                .Where(e => e.AFirstStringProperty, ConditionOperator.EqualTo, "avalue");

            result.AllEntities.Count.Should().Be(2);
            result.PrimaryEntity.Join.Should().BeNull();
            result.AllEntities[1].Join.Left.EntityName.Should().Be("first");
            result.AllEntities[1].Join.Left.JoinedFieldName.Should().Be("AFirstStringProperty");
            result.AllEntities[1].Join.Right.EntityName.Should().Be("second");
            result.AllEntities[1].Join.Right.JoinedFieldName.Should().Be("ASecondStringProperty");
            result.AllEntities[1].Join.Type.Should().Be(JoinType.Inner);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenJoinMultipleEntities_ThenCreatesJoins()
        {
            var result = Query.From<FirstTestEntity>()
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty,
                    JoinType.Left)
                .AndJoin<ThirdTestEntity, DateTime>(f => f.AFirstDateTimeProperty, t => t.AThirdDateTimeProperty)
                .Where(e => e.AFirstStringProperty, ConditionOperator.EqualTo, "avalue");

            result.AllEntities.Count.Should().Be(3);
            result.PrimaryEntity.Join.Should().BeNull();
            result.AllEntities[1].Join.Left.EntityName.Should().Be("first");
            result.AllEntities[1].Join.Left.JoinedFieldName.Should().Be("AFirstStringProperty");
            result.AllEntities[1].Join.Right.EntityName.Should().Be("second");
            result.AllEntities[1].Join.Right.JoinedFieldName.Should().Be("ASecondStringProperty");
            result.AllEntities[1].Join.Type.Should().Be(JoinType.Left);

            result.AllEntities[2].Join.Left.EntityName.Should().Be("first");
            result.AllEntities[2].Join.Left.JoinedFieldName.Should().Be("AFirstDateTimeProperty");
            result.AllEntities[2].Join.Right.EntityName.Should().Be("third");
            result.AllEntities[2].Join.Right.JoinedFieldName.Should().Be("AThirdDateTimeProperty");
            result.AllEntities[2].Join.Type.Should().Be(JoinType.Inner);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenJoinWithMultipleJoinsWithSameType_ThenThrows()
        {
            Query.From<FirstTestEntity>()
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty)
                .Invoking(x =>
                    x.AndJoin<SecondTestEntity, DateTime>(f => f.AFirstDateTimeProperty,
                        t => t.ASecondDateTimeProperty))
                .Should().Throw<InvalidOperationException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenJoinWithMultipleJoinsOnSameProperty_ThenCreatesJoins()
        {
            var result = Query.From<FirstTestEntity>()
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty,
                    JoinType.Left)
                .AndJoin<ThirdTestEntity, string>(f => f.AFirstStringProperty, t => t.AThirdStringProperty)
                .Where(e => e.AFirstStringProperty, ConditionOperator.EqualTo, "avalue");

            result.AllEntities.Count.Should().Be(3);
            result.PrimaryEntity.Join.Should().BeNull();
            result.AllEntities[1].Join.Left.EntityName.Should().Be("first");
            result.AllEntities[1].Join.Left.JoinedFieldName.Should().Be("AFirstStringProperty");
            result.AllEntities[1].Join.Right.EntityName.Should().Be("second");
            result.AllEntities[1].Join.Right.JoinedFieldName.Should().Be("ASecondStringProperty");
            result.AllEntities[1].Join.Type.Should().Be(JoinType.Left);

            result.AllEntities[2].Join.Left.EntityName.Should().Be("first");
            result.AllEntities[2].Join.Left.JoinedFieldName.Should().Be("AFirstStringProperty");
            result.AllEntities[2].Join.Right.EntityName.Should().Be("third");
            result.AllEntities[2].Join.Right.JoinedFieldName.Should().Be("AThirdStringProperty");
            result.AllEntities[2].Join.Type.Should().Be(JoinType.Inner);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenNoSelect_ThenSelectsAllFields()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue");

            result.PrimaryEntity.Selects.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSelectWithFromEntityField_ThenFieldSelected()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue")
                .Select(e => e.ADateTimeProperty);

            result.PrimaryEntity.Selects.Count.Should().Be(1);
            result.PrimaryEntity.Selects[0].EntityName.Should().Be("aname");
            result.PrimaryEntity.Selects[0].FieldName.Should().Be("ADateTimeProperty");
            result.PrimaryEntity.Selects[0].JoinedEntityName.Should().BeNull();
            result.PrimaryEntity.Selects[0].JoinedFieldName.Should().BeNull();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSelectWithFromEntityFields_ThenFieldsSelected()
        {
            var result = Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue")
                .Select(e => e.AStringProperty).Select(e => e.ADateTimeProperty);

            result.PrimaryEntity.Selects.Count.Should().Be(2);
            result.PrimaryEntity.Selects[0].EntityName.Should().Be("aname");
            result.PrimaryEntity.Selects[0].FieldName.Should().Be("AStringProperty");
            result.PrimaryEntity.Selects[0].JoinedEntityName.Should().BeNull();
            result.PrimaryEntity.Selects[0].JoinedFieldName.Should().BeNull();
            result.PrimaryEntity.Selects[1].EntityName.Should().Be("aname");
            result.PrimaryEntity.Selects[1].FieldName.Should().Be("ADateTimeProperty");
            result.PrimaryEntity.Selects[1].JoinedEntityName.Should().BeNull();
            result.PrimaryEntity.Selects[1].JoinedFieldName.Should().BeNull();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSelectFromJoinAndNoJoins_ThenThrows()
        {
            Query.From<NamedTestEntity>()
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue")
                .Invoking(x =>
                    x.SelectFromJoin<SecondTestEntity, string>(e => e.AStringProperty, s => s.ASecondStringProperty))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueryClause_SelectFromJoin_NoJoins);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSelectFromJoinAndUnknownJoins_ThenThrows()
        {
            Query.From<NamedTestEntity>()
                .Join<FirstTestEntity, string>(e => e.AStringProperty, f => f.AFirstStringProperty)
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue")
                .Invoking(x =>
                    x.SelectFromJoin<SecondTestEntity, string>(e => e.AStringProperty, s => s.ASecondStringProperty))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueryClause_SelectFromJoin_UnknownJoin);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSelectFromJoinAndExistingJoin_TheFieldSelected()
        {
            var result = Query.From<NamedTestEntity>()
                .Join<FirstTestEntity, string>(e => e.AStringProperty, f => f.AFirstStringProperty)
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue")
                .SelectFromJoin<FirstTestEntity, string>(e => e.AStringProperty, s => s.AFirstStringProperty);

            result.PrimaryEntity.Selects.Count.Should().Be(0);
            result.AllEntities[1].Selects.Count.Should().Be(1);
            result.AllEntities[1].Selects[0].EntityName.Should().Be("first");
            result.AllEntities[1].Selects[0].FieldName.Should().Be("AFirstStringProperty");
            result.AllEntities[1].Selects[0].JoinedEntityName.Should().Be("aname");
            result.AllEntities[1].Selects[0].JoinedFieldName.Should().Be("AStringProperty");
        }
    }

    public class UnnamedTestEntity : IQueryableEntity
    {
    }

    public class UnnamedTestEntityUnconventionalNamed : IQueryableEntity
    {
    }

    [EntityName("aname")]
    public class NamedTestEntity : IQueryableEntity
    {
        public string AStringProperty => null;

        public DateTime ADateTimeProperty => default;
    }

    [EntityName("first")]
    public class FirstTestEntity : IQueryableEntity
    {
        public string AFirstStringProperty => null;
        public DateTime AFirstDateTimeProperty => DateTime.MinValue;
    }

    [EntityName("second")]
    public class SecondTestEntity : IQueryableEntity
    {
        public string ASecondStringProperty => null;
        public DateTime ASecondDateTimeProperty => DateTime.MinValue;
    }

    [EntityName("third")]
    public class ThirdTestEntity : IQueryableEntity
    {
        public string AThirdStringProperty => null;
        public DateTime AThirdDateTimeProperty => DateTime.MinValue;
    }
}