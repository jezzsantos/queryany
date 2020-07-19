using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueryAny;
using Storage.Azure;

namespace Storage.UnitTests
{
    [TestClass]
    public class AzureCosmosSqlApiWhereExtensionsSpec
    {
        private Mock<IRepository> repository;

        [TestInitialize]
        public void Initialize()
        {
            this.repository = new Mock<IRepository>();
            this.repository.Setup(repo => repo.MaxQueryResults)
                .Returns(99);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndNoSelects_ThenReturnsSqlExpression()
        {
            var query = Query.Empty<TestEntity>();

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername", this.repository.Object);

            result.Should().Be("SELECT * FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndSingleSelect_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue);

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername", this.repository.Object);

            result.Should()
                .Be("SELECT t.id, t.ABooleanValue FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndMultipleSelects_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue)
                .Select(e => e.ADoubleValue);

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT t.id, t.ABooleanValue, t.ADoubleValue FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }
    }
}