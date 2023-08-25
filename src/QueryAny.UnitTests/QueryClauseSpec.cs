using System;
using FluentAssertions;
using QueryAny.Properties;
using Xunit;

namespace QueryAny.UnitTests
{
    [Trait("Category", "Unit")]
    public class QueryClauseSpec
    {
        [Fact]
        public void WhenFromWithUnnamedEntityType_ThenCreatesADerivedNamedCollection()
        {
            var result = Query.From<UnnamedTestEntity>();

            result.PrimaryEntity.EntityName.Should().Be("UnnamedTest");
            result.PrimaryEntity.EntityName.Should().Be("UnnamedTest");
        }

        [Fact]
        public void WhenFromWithNamedEntityType_ThenCreatesANamedCollection()
        {
            var result = Query.From<NamedTestEntity>();

            result.PrimaryEntity.EntityName.Should().Be("aname");
        }

        [Fact]
        public void WhenFromWithUnnamedUnconventionallyNamedType_ThenCreatesDefaultNamedCollection()
        {
            var result = Query.From<UnnamedTestEntityUnconventionalNamed>();

            result.PrimaryEntity.EntityName.Should().Be("unnamedtestentityunconventionalnamed");
        }

        [Fact]
        public void WhenEmpty_ThenCreatesNoWheres()
        {
            var result = Query.Empty<NamedTestEntity>();

            result.PrimaryEntity.EntityName.Should().Be("aname");
            result.Wheres.Count.Should().Be(0);
        }

        [Fact]
        public void WhenEmpty_ThenLimitIsDefaultLimit()
        {
            var result = Query.Empty<NamedTestEntity>();

            result.ResultOptions.Limit.Should().Be(ResultOptions.DefaultLimit);
        }

        [Fact]
        public void WhenFrom_ThenLimitIsDefaultLimit()
        {
            var result = Query.From<NamedTestEntity>();

            result.ResultOptions.Limit.Should().Be(ResultOptions.DefaultLimit);
        }

        [Fact]
        public void WhenEmpty_ThenOffsetIsDefaultLimit()
        {
            var result = Query.Empty<NamedTestEntity>();

            result.ResultOptions.Offset.Should().Be(ResultOptions.DefaultOffset);
        }

        [Fact]
        public void WhenFrom_ThenOffsetIsDefaultLimit()
        {
            var result = Query.From<NamedTestEntity>();

            result.ResultOptions.Offset.Should().Be(ResultOptions.DefaultOffset);
        }

        [Fact]
        public void WhenEmpty_ThenOrderIsDefaultOrder()
        {
            var result = Query.Empty<NamedTestEntity>();

            result.ResultOptions.OrderBy.By.Should().Be(ResultOptions.DefaultOrder);
            result.ResultOptions.OrderBy.Direction.Should().Be(ResultOptions.DefaultOrderDirection);
        }

        [Fact]
        public void WhenFrom_ThenOrderIsDefaultOrder()
        {
            var result = Query.From<NamedTestEntity>();

            result.ResultOptions.OrderBy.By.Should().Be(ResultOptions.DefaultOrder);
            result.ResultOptions.OrderBy.Direction.Should().Be(ResultOptions.DefaultOrderDirection);
        }

        [Fact]
        public void WhenAndWhereBeforeWhere_ThenThrows()
        {
            var query = Query.Empty<NamedTestEntity>();

            query
                .Invoking(x => x.AndWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "1"))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueryClause_AndWhereBeforeWheres);
        }

        [Fact]
        public void WhenAndWhereAfterWhereNoOp_ThenCreatesAnAndedWhere()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .WhereNoOp();

            var result = clause
                .AndWhere(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2");

            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.And);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("2");
        }

        [Fact]
        public void WhenAndWhereAfterWhereAll_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query.WhereAll();

            clause
                .Invoking(x => x.AndWhere(entity => entity.AStringProperty, ConditionOperator.EqualTo, "avalue"))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.QueryClause_AndWhereBeforeWheres);
        }

        [Fact]
        public void WhenAndWhereWithStringProperty_ThenCreatesAnAndedWhere()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            var result = clause
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

        [Fact]
        public void WhenAndWhereWithDateTimeProperty_ThenCreatesAnAndedWhere()
        {
            var datum = DateTime.UtcNow;
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            var result = clause
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

        [Fact]
        public void WhenOrWhereBeforeWhere_ThenThrows()
        {
            var query = Query.Empty<NamedTestEntity>();

            query
                .Invoking(x => x.OrWhere(e => e.AStringProperty, ConditionOperator.EqualTo, "1"))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueryClause_OrWhereBeforeWheres);
        }

        [Fact]
        public void WhenOrWhereAfterWhereNoOp_ThenCreatesAnOredWhere()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .WhereNoOp();

            var result = clause
                .OrWhere(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2");

            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.Or);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.NotEqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be("AStringProperty");
            result.Wheres[0].Condition.Value.Should().Be("2");
        }

        [Fact]
        public void WhenOrWhereAfterWhereAll_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query.WhereAll();

            clause
                .Invoking(x => x.OrWhere(entity => entity.AStringProperty, ConditionOperator.EqualTo, "avalue"))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.QueryClause_OrWhereBeforeWheres);
        }

        [Fact]
        public void WhenOrWhereWithStringProperty_ThenCreatesAnOredWhere()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            var result = clause
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

        [Fact]
        public void WhenOrWhereWithDateTimeProperty_ThenCreatesAnOredWhere()
        {
            var datum = DateTime.UtcNow;
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            var result = clause
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

        [Fact]
        public void WhenAndWhereWithSubWhereClause_ThenCreatesAnAndedNestedWhere()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            var result = clause
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

        [Fact]
        public void WhenOrWhereWithSubWhereClauseAndNoWheres_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .WhereAll();

            clause
                .Invoking(x => x.OrWhere(sub => sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2")))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueryClause_OrWhereBeforeWheres);
        }

        [Fact]
        public void WhenOrWhereWithSubWhereClause_ThenCreatesAnOredNestedWhere()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            var result = clause
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

        [Fact]
        public void WhenAndWhereWithSubWhereClauseAndNoWheres_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .WhereAll();

            clause
                .Invoking(x => x.AndWhere(sub => sub.Where(e => e.AStringProperty, ConditionOperator.NotEqualTo, "2")))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueryClause_AndWhereBeforeWheres);
        }

        [Fact]
        public void WhenAndWhereWithSubWhereClauses_ThenCreatesAnAndedNestedWheres()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            var result = clause
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

        [Fact]
        public void WhenNoSelects_ThenNoSelectedFields()
        {
            var query = Query.From<NamedTestEntity>();

            var result = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue");

            result.PrimaryEntity.Selects.Count.Should().Be(0);
        }

        [Fact]
        public void WhenSelectWithFromEntityField_ThenFieldSelected()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue");

            var result = clause
                .Select(e => e.ADateTimeProperty);

            result.PrimaryEntity.Selects.Count.Should().Be(1);
            result.PrimaryEntity.Selects[0].EntityName.Should().Be("aname");
            result.PrimaryEntity.Selects[0].FieldName.Should().Be("ADateTimeProperty");
            result.PrimaryEntity.Selects[0].JoinedEntityName.Should().BeNull();
            result.PrimaryEntity.Selects[0].JoinedFieldName.Should().BeNull();
        }

        [Fact]
        public void WhenSelectWithFromEntityFields_ThenFieldsSelected()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue");

            var result = clause
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

        [Fact]
        public void WhenSelectFromJoinAndNoJoins_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue");

            clause
                .Invoking(x =>
                    x.SelectFromJoin<SecondTestEntity, string>(e => e.AStringProperty, s => s.ASecondStringProperty))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueryClause_SelectFromJoin_NoJoins);
        }

        [Fact]
        public void WhenSelectFromJoinAndUnknownJoins_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Join<FirstTestEntity, string>(e => e.AStringProperty, f => f.AFirstStringProperty)
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue");

            clause
                .Invoking(x =>
                    x.SelectFromJoin<SecondTestEntity, string>(e => e.AStringProperty, s => s.ASecondStringProperty))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueryClause_SelectFromJoin_UnknownJoin);
        }

        [Fact]
        public void WhenSelectFromJoinAndExistingJoin_TheFieldSelected()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query
                .Join<FirstTestEntity, string>(e => e.AStringProperty, f => f.AFirstStringProperty)
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "avalue");

            var result = clause
                .SelectFromJoin<FirstTestEntity, string>(e => e.AStringProperty, s => s.AFirstStringProperty);

            result.PrimaryEntity.Selects.Count.Should().Be(0);
            result.AllEntities[1].Selects.Count.Should().Be(1);
            result.AllEntities[1].Selects[0].EntityName.Should().Be("first");
            result.AllEntities[1].Selects[0].FieldName.Should().Be("AFirstStringProperty");
            result.AllEntities[1].Selects[0].JoinedEntityName.Should().Be("aname");
            result.AllEntities[1].Selects[0].JoinedFieldName.Should().Be("AStringProperty");
        }

        [Fact]
        public void WhenTake_ThenSetsLimit()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query.WhereAll();

            var result = clause.Take(10);

            result.ResultOptions.Limit.Should().Be(10);
        }

        [Fact]
        public void WhenSkip_ThenSetsOffset()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query.WhereAll();

            var result = clause.Skip(10);

            result.ResultOptions.Offset.Should().Be(10);
        }

        [Fact]
        public void WhenOrderBy_ThenSetsOrder()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query.WhereAll();

            var result = clause.OrderBy(e => e.AStringProperty);

            result.ResultOptions.OrderBy.By.Should().Be("AStringProperty");
            result.ResultOptions.OrderBy.Direction.Should().Be(OrderDirection.Ascending);
        }
    }
}