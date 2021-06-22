using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using QueryAny;
using ServiceStack;
using Storage.Azure;
using Xunit;

namespace Storage.IntegrationTests.Azure
{
    public class AzureCosmosTableApiRepositorySpecSetup : IDisposable
    {
        public AzureCosmosTableApiRepositorySpecSetup()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            Repository = AzureCosmosTableApiRepository.FromSettings(settings);
            AzureCosmosStorageBase.InitializeAllTests(null);
        }

        public IRepository Repository { get; }

        public void Dispose()
        {
            AzureCosmosStorageBase.CleanupAllTests();
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class AzureCosmosTableApiRepositorySpec : AnyRepositoryBaseSpec,
        IClassFixture<AzureCosmosTableApiRepositorySpecSetup>
    {
        public AzureCosmosTableApiRepositorySpec(AzureCosmosTableApiRepositorySpecSetup setup) : base(setup.Repository)
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