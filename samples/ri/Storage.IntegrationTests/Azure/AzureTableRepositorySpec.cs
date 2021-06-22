using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using QueryAny;
using ServiceStack;
using Storage.Azure;
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
        public AzureTableRepositorySpec(AzureTableRepositorySpecSetup setup) : base(setup.Repository)
        {
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