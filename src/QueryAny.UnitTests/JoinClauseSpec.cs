using System;
using FluentAssertions;
using QueryAny.Extensions;
using QueryAny.Properties;
using Xunit;

namespace QueryAny.UnitTests
{
    [Trait("Category", "Unit")]
    public class JoinClauseSpec
    {
        [Fact]
        public void WhenJoinMultipleEntities_ThenCreatesJoins()
        {
            var query = Query.From<FirstTestEntity>();

            var result = query
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

        [Fact]
        public void WhenJoinWithMultipleJoinsWithSameType_ThenThrows()
        {
            var query = Query.From<FirstTestEntity>();
            var join = query.Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty);

            join
                .Invoking(x =>
                    x.AndJoin<SecondTestEntity, DateTime>(f => f.AFirstDateTimeProperty,
                        t => t.ASecondDateTimeProperty))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueriedEntities_JoinSameEntity.Format("second"));
        }

        [Fact]
        public void WhenJoinWithMultipleJoinsOnSameProperty_ThenCreatesJoins()
        {
            var query = Query.From<FirstTestEntity>();
            var result = query
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

        [Fact]
        public void WhenWhereAllAndHasWhereNoOp_ThenThrows()
        {
            var query = Query.From<FirstTestEntity>();
            var clause = query
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty);
            clause.WhereNoOp();

            clause
                .Invoking(x => x.WhereAll())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAllAndNotEmpty);
        }

        [Fact]
        public void WhenWhereAllAndHasWheres_ThenThrows()
        {
            var query = Query.From<FirstTestEntity>();
            var clause = query
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty);
            clause.Where(entity => entity.AFirstStringProperty, ConditionOperator.EqualTo, "avalue");

            query
                .Invoking(x => x.WhereAll())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAllAndNotEmpty);
        }

        [Fact]
        public void WhenWhereAllAndHasWhereAll_ThenThrows()
        {
            var query = Query.From<FirstTestEntity>();
            var clause = query
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty);
            clause.WhereAll();

            clause
                .Invoking(x => x.WhereAll())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereAllAndNotEmpty);
        }

        [Fact]
        public void WhenWhereAll_ThenReturnsQueryClauseWithAllWheres()
        {
            var query = Query.From<FirstTestEntity>();
            var clause = query
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty);

            var result = clause.WhereAll();

            result.Wheres.Count.Should().Be(0);
            result.Options.Wheres.Should().Be(WhereOptions.AllDefined);
        }

        [Fact]
        public void WhenWhereNoOpAndHasWhereNoOp_ThenThrows()
        {
            var query = Query.From<FirstTestEntity>();
            var clause = query
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty);
            clause.WhereNoOp();

            clause
                .Invoking(x => x.WhereNoOp())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereNoOpAndNotEmpty);
        }

        [Fact]
        public void WhenWhereNoOpAndHasWheres_ThenThrows()
        {
            var query = Query.From<FirstTestEntity>();
            var clause = query
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty);
            clause.Where(entity => entity.AFirstStringProperty, ConditionOperator.EqualTo, "avalue");

            clause
                .Invoking(x => x.WhereNoOp())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(Resources.FromClause_WhereNoOpAndNotEmpty);
        }

        [Fact]
        public void WhenWhereNoOp_ThenReturnsQueryClauseWithSpecificWheres()
        {
            var query = Query.From<FirstTestEntity>();
            var clause = query
                .Join<SecondTestEntity, string>(f => f.AFirstStringProperty, s => s.ASecondStringProperty);

            var result = clause.WhereNoOp();

            result.Wheres.Count.Should().Be(0);
            result.Options.Wheres.Should().Be(WhereOptions.SomeDefined);
        }
    }
}