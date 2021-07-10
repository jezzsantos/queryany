using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using QueryAny;
using ServiceStack;
using Storage.Azure;
using Storage.Azure.Properties;
using UnitTesting.Common;
using Xunit;

namespace Storage.IntegrationTests.Azure
{
    public class AzureTableRepositorySpecSetup : IDisposable
    {
        public AzureTableRepositorySpecSetup()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            Repository = AzureTableStorageRepository.FromSettings(settings);
            AzureStorageAccountBase.InitializeAllTests();
        }

        public IRepository Repository { get; }

        public void Dispose()
        {
            AzureStorageAccountBase.CleanupAllTests();
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class AzureTableRepositorySpec : AnyRepositoryBaseSpec, IClassFixture<AzureTableRepositorySpecSetup>
    {
        private readonly AzureTableRepositorySpecSetup setup;

        public AzureTableRepositorySpec(AzureTableRepositorySpecSetup setup) : base(setup.Repository)
        {
            this.setup = setup;
        }

        [Fact]
        public void WhenAddWithInvalidTableName_ThenThrows()
        {
            this.setup.Repository
                .Invoking(x => x.Add("^aninvalidtablename^", new CommandEntity("anid")))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenRemoveWithInvalidTableName_ThenThrows()
        {
            this.setup.Repository
                .Invoking(x => x.Remove("^aninvalidtablename^", "anid"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenReplaceWithInvalidTableName_ThenThrows()
        {
            this.setup.Repository
                .Invoking(x => x.Replace("^aninvalidtablename^", "anid", new CommandEntity("anid")))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenRetrieveWithInvalidTableName_ThenThrows()
        {
            this.setup.Repository
                .Invoking(x => x.Retrieve("^aninvalidtablename^", "anid", RepositoryEntityMetadata.Empty))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenCountWithInvalidTableName_ThenThrows()
        {
            this.setup.Repository
                .Invoking(x => x.Count("^aninvalidtablename^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenDestroyAllWithInvalidTableName_ThenThrows()
        {
            this.setup.Repository
                .Invoking(x => x.DestroyAll("^aninvalidtablename^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public override void WhenQueryForStringValueWithLikeExact_ThenReturnsResult()
        {
            var query = Query.From<TestRepositoryEntity>()
                .Where(e => e.AStringValue, ConditionOperator.Like, "value");

            Repository
                .Invoking(x => x.Query(ContainerName, query,
                    RepositoryEntityMetadata.FromType<TestRepositoryEntity>()))
                .Should().Throw<NotSupportedException>();
        }

        [Fact]
        public override void WhenQueryForStringValueWithLikePartial_ThenReturnsResult()
        {
            var query = Query.From<TestRepositoryEntity>()
                .Where(e => e.AStringValue, ConditionOperator.Like, "value");

            Repository
                .Invoking(x => x.Query(ContainerName, query,
                    RepositoryEntityMetadata.FromType<TestRepositoryEntity>()))
                .Should().Throw<NotSupportedException>();
        }
    }
}