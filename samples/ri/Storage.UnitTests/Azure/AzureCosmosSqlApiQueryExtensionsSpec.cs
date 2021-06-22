using FluentAssertions;
using Moq;
using QueryAny;
using Storage.Azure;
using Xunit;

namespace Storage.UnitTests.Azure
{
    // ReSharper disable once InconsistentNaming
    [Trait("Category", "Unit")]
    public class AzureCosmosSqlApiQueryExtensionsSpec
    {
        private readonly Mock<IRepository> repository;

        public AzureCosmosSqlApiQueryExtensionsSpec()
        {
            this.repository = new Mock<IRepository>();
            this.repository.Setup(repo => repo.MaxQueryResults)
                .Returns(99);
        }

        [Fact]
        public void WhenToAzureCosmosSqlApiQueryClauseAndNoSelects_ThenReturnsSqlExpression()
        {
            var query = Query.Empty<TestQueryEntity>();

            var result = query.ToCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should().Be("SELECT * FROM acontainername t ORDER BY t.AStringValue ASC OFFSET 0 LIMIT 99");
        }

        [Fact]
        public void WhenToAzureCosmosSqlApiQueryClauseAndSingleSelect_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestQueryEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue);

            var result = query.ToCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT t.id, t.ABooleanValue FROM acontainername t ORDER BY t.ABooleanValue ASC OFFSET 0 LIMIT 99");
        }

        [Fact]
        public void WhenToAzureCosmosSqlApiQueryClauseAndMultipleSelects_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestQueryEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue)
                .Select(e => e.ADoubleValue);

            var result = query.ToCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT t.id, t.ABooleanValue, t.ADoubleValue FROM acontainername t ORDER BY t.ABooleanValue ASC OFFSET 0 LIMIT 99");
        }
    }
}