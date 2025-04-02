using System;
using System.Linq;
using FluentAssertions;
using QueryAny.Properties;
using Xunit;

namespace QueryAny.UnitTests
{
    [Trait("Category", "Unit")]
    public class FromClauseSpec
    {
        [Fact]
        public void WhenWhereAndHasWhereNoOp_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            query.WhereNoOp();

            query
                .Invoking(x => x.Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1"))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAndNotEmpty);
        }

        [Fact]
        public void WhenWhereAndHasWhereAll_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            query.WhereAll();

            query
                .Invoking(x => x.Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1"))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAndNotEmpty);
        }

        [Fact]
        public void WhenWhereAndHasWhere_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            query.Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            query
                .Invoking(x => x.Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1"))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAndNotEmpty);
        }

        [Fact]
        public void WhenWhereWithStringProperty_ThenCreatesAWhere()
        {
            var query = Query.From<NamedTestEntity>();

            var result = query
                .Where(e => e.AStringProperty, ConditionOperator.EqualTo, "1");

            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be(nameof(NamedTestEntity.AStringProperty));
            result.Wheres[0].Condition.Value.Should().Be("1");
        }

        [Fact]
        public void WhenWhereWithDateTimeProperty_ThenCreatesAWhere()
        {
            var datum = DateTime.UtcNow;
            var query = Query.From<NamedTestEntity>();

            var result = query
                .Where(e => e.ADateTimeProperty, ConditionOperator.EqualTo, datum);

            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be(nameof(NamedTestEntity.ADateTimeProperty));
            result.Wheres[0].Condition.Value.Should().Be(datum);
        }

        [Fact]
        public void WhenWhereWithArrayProperty_ThenCreatesAWhere()
        {
            var array = new[] { "avalue1", "avalue2" }.ToArray();
            var query = Query.From<NamedTestEntity>();

            var result = query
                .Where(e => e.AStringProperty, ConditionOperator.IsIn, array);

            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.IsIn);
            result.Wheres[0].Condition.FieldName.Should().Be(nameof(NamedTestEntity.AStringProperty));
            result.Wheres[0].Condition.Value.Should().Be(array);
        }

        [Fact]
        public void WhenWhereAllAndHasWhereNoOp_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            query.WhereNoOp();

            query
                .Invoking(x => x.WhereAll())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAllAndNotEmpty);
        }

        [Fact]
        public void WhenWhereAllAndHasWheres_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            query.Where(entity => entity.AStringProperty, ConditionOperator.EqualTo, "avalue");

            query
                .Invoking(x => x.WhereAll())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAllAndNotEmpty);
        }

        [Fact]
        public void WhenWhereAllAndHasWhereAll_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            query.WhereAll();

            query
                .Invoking(x => x.WhereAll())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAllAndNotEmpty);
        }

        [Fact]
        public void WhenWhereAll_ThenReturnsQueryClauseWithAllWheres()
        {
            var query = Query.From<NamedTestEntity>();

            var result = query.WhereAll();

            result.Wheres.Count.Should().Be(0);
            result.Options.Wheres.Should().Be(WhereOptions.AllDefined);
        }

        [Fact]
        public void WhenWhereNoOpAndHasWhereNoOp_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            query.WhereNoOp();

            query
                .Invoking(x => x.WhereNoOp())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereNoOpAndNotEmpty);
        }

        [Fact]
        public void WhenWhereNoOpAndHasWheres_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            query.Where(entity => entity.AStringProperty, ConditionOperator.EqualTo, "avalue");

            query
                .Invoking(x => x.WhereNoOp())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereNoOpAndNotEmpty);
        }

        [Fact]
        public void WhenWhereNoOp_ThenReturnsQueryClauseWithSpecificWheres()
        {
            var query = Query.From<NamedTestEntity>();

            var result = query.WhereNoOp();

            result.Wheres.Count.Should().Be(0);
            result.Options.Wheres.Should().Be(WhereOptions.SomeDefined);
        }

        [Fact]
        public void WhenWhereNoOpFollowedByAndWhere_ThenReturnsQueryClauseWithSpecificWheres()
        {
            var query = Query.From<NamedTestEntity>();

            var result = query.WhereNoOp()
                .AndWhere(entity => entity.AStringProperty, ConditionOperator.EqualTo, "avalue");

            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be(nameof(NamedTestEntity.AStringProperty));
            result.Wheres[0].Condition.Value.Should().Be("avalue");
            result.Options.Wheres.Should().Be(WhereOptions.SomeDefined);
        }

        [Fact]
        public void WhenWhereNoOpFollowedByOrWhere_ThenReturnsQueryClauseWithSpecificWheres()
        {
            var query = Query.From<NamedTestEntity>();

            var result = query.WhereNoOp()
                .OrWhere(entity => entity.AStringProperty, ConditionOperator.EqualTo, "avalue");
            result.Wheres.Count.Should().Be(1);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be(nameof(NamedTestEntity.AStringProperty));
            result.Wheres[0].Condition.Value.Should().Be("avalue");
            result.Options.Wheres.Should().Be(WhereOptions.SomeDefined);
        }

        [Fact]
        public void WhenWhereNoOpFollowedByMultipleAndWheres_ThenReturnsQueryClauseWithSpecificWheres()
        {
            var query = Query.From<NamedTestEntity>();

            var result = query.WhereNoOp()
                .AndWhere(entity => entity.AStringProperty, ConditionOperator.EqualTo, "avalue1")
                .AndWhere(entity => entity.AStringProperty, ConditionOperator.EqualTo, "avalue2");

            result.Wheres.Count.Should().Be(2);
            result.Wheres[0].Operator.Should().Be(LogicalOperator.None);
            result.Wheres[0].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[0].Condition.FieldName.Should().Be(nameof(NamedTestEntity.AStringProperty));
            result.Wheres[0].Condition.Value.Should().Be("avalue1");
            result.Wheres[1].Operator.Should().Be(LogicalOperator.And);
            result.Wheres[1].Condition.Operator.Should().Be(ConditionOperator.EqualTo);
            result.Wheres[1].Condition.FieldName.Should().Be(nameof(NamedTestEntity.AStringProperty));
            result.Wheres[1].Condition.Value.Should().Be("avalue2");
            result.Options.Wheres.Should().Be(WhereOptions.SomeDefined);
        }

        [Fact]
        public void WhenJoin_ThenCreatesAnInnerJoin()
        {
            var query = Query.From<FirstTestEntity>();
            var result = query
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

        [Fact]
        public void WhenTake_ThenLimitIsSet()
        {
            var query = Query.From<NamedTestEntity>();

            var results = query
                .Take(10);

            results.ResultOptions.Limit.Should().Be(10);
        }

        [Fact]
        public void WhenSkip_ThenLimitIsSet()
        {
            var query = Query.From<NamedTestEntity>();

            var results = query
                .Skip(10);

            results.ResultOptions.Offset.Should().Be(10);
        }

        [Fact]
        public void WhenOrderBy_ThenSetsOrder()
        {
            var query = Query.From<NamedTestEntity>();

            var result = query.OrderBy(e => e.AStringProperty);

            result.ResultOptions.OrderBy.By.Should().Be(nameof(NamedTestEntity.AStringProperty));
            result.ResultOptions.OrderBy.Direction.Should().Be(OrderDirection.Ascending);
        }
    }
}