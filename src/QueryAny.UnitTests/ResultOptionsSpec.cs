using System;
using FluentAssertions;
using QueryAny.Properties;
using Xunit;

namespace QueryAny.UnitTests
{
    [Trait("Category", "Unit")]
    public class ResultOptionsSpec
    {
        [Fact]
        public void WhenTakeWithNegativeNumber_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();

            query
                .Invoking(x => x.Take(-1))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.ResultOptions_InvalidLimit);
        }

        [Fact]
        public void WhenSkipWithNegativeNumber_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();

            query
                .Invoking(x => x.Skip(-1))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.ResultOptions_InvalidOffset);
        }

        [Fact]
        public void WhenOrderByWithNull_ThenThrows()
        {
            var query = Query.From<NamedTestEntity>();

            query
                .Invoking(x => x.OrderBy<string>(e => null))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.ResultOptions_InvalidOrderBy);
        }
    }
}