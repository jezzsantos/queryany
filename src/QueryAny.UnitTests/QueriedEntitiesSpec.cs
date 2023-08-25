using System;
using FluentAssertions;
using QueryAny.Properties;
using Xunit;

namespace QueryAny.UnitTests
{
    [Trait("Category", "Unit")]
    public class QueriedEntitiesSpec
    {
        [Fact]
        public void WhenTakeAgain_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query.Take(10);

            clause
                .Invoking(x => x.Take(10))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueriedEntities_LimitAlreadySet);
        }

        [Fact]
        public void WhenSkipAgain_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query.Skip(10);

            clause
                .Invoking(x => x.Skip(10))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueriedEntities_OffsetAlreadySet);
        }

        [Fact]
        public void WhenOrderByAgain_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();
            var clause = query.OrderBy(e => e.AStringProperty);

            clause
                .Invoking(x => x.OrderBy(e => e.AStringProperty))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.QueriedEntities_OrderByAlreadySet);
        }
    }
}