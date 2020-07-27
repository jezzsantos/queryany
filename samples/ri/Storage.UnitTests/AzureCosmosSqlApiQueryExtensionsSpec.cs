using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueryAny;
using Storage.Azure;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class AzureCosmosSqlApiQueryExtensionsSpec
    {
        private Mock<IRepository> repository;

        [TestInitialize]
        public void Initialize()
        {
            this.repository = new Mock<IRepository>();
            this.repository.Setup(repo => repo.MaxQueryResults)
                .Returns(99);
        }

        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndNoSelects_ThenReturnsSqlExpression()
        {
            var query = Query.Empty<TestEntity>();

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should().Be("SELECT * FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndSingleSelect_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue);

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be("SELECT t.id, t.ABooleanValue FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndMultipleSelects_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue)
                .Select(e => e.ADoubleValue);

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT t.id, t.ABooleanValue, t.ADoubleValue FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndDistinctByForUnknownSelect_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue)
                .DistinctBy(e => e.AStringValue);

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT t.id, t.ABooleanValue FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndDistinctByIdForSelect_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.Id)
                .Select(e => e.ABooleanValue)
                .DistinctBy(e => e.Id);

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT DISTINCT t.id, t.ABooleanValue FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndDistinctByForSelect_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.AStringValue)
                .Select(e => e.ABooleanValue)
                .DistinctBy(e => e.AStringValue);

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT DISTINCT t.AStringValue, t.id, t.ABooleanValue FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndUnknownDistinctByForNoSelects_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .DistinctBy(e => e.AnInternalProperty);

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT * FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }


        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndDistinctByIdForNoSelects_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .DistinctBy(e => e.Id);

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT DISTINCT t.id, t.AStringValue, t.ABooleanValue, t.ADoubleValue, t.CreatedAtUtc, t.LastModifiedAtUtc FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }

        [TestMethod]
        public void WhenToAzureCosmosSqlApiQueryClauseAndDistinctByForNoSelects_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .DistinctBy(e => e.AStringValue);

            var result = query.ToAzureCosmosSqlApiQueryClause("acontainername", this.repository.Object);

            result.Should()
                .Be(
                    "SELECT DISTINCT t.AStringValue, t.id, t.ABooleanValue, t.ADoubleValue, t.CreatedAtUtc, t.LastModifiedAtUtc FROM acontainername t ORDER BY t.CreatedAtUtc ASC OFFSET 0 LIMIT 99");
        }
    }
}