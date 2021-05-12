using System;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Azure;

namespace Storage.IntegrationTests.Azure
{
    [TestClass, TestCategory("Integration.Storage")]
    public class AzureCosmosTableApiRepositorySpec : AnyRepositoryBaseSpec
    {
        private static AzureCosmosTableApiRepository repository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            repository = AzureCosmosTableApiRepository.FromSettings(settings);
            AzureCosmosStorageBase.InitializeAllTests(context, "/EnableTableEndpoint");
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            AzureCosmosStorageBase.CleanupAllTests();
        }

        protected override RepoInfo GetRepository<TEntity>()
        {
            return new RepoInfo
            {
                Repository = repository,
                ContainerName = typeof(TEntity).GetEntityNameSafe()
            };
        }

        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public override void WhenQueryForStringValueWithLikeExact_ThenReturnsResult()
        {
            var query = Query.From<TestRepositoryEntity>()
                .Where(e => e.AStringValue, ConditionOperator.Like, "value");

            Repository.Query(ContainerName, query,
                RepositoryEntityMetadata.FromType<TestRepositoryEntity>());
        }

        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public override void WhenQueryForStringValueWithLikePartial_ThenReturnsResult()
        {
            var query = Query.From<TestRepositoryEntity>()
                .Where(e => e.AStringValue, ConditionOperator.Like, "value");

            Repository.Query(ContainerName, query,
                RepositoryEntityMetadata.FromType<TestRepositoryEntity>());
        }
    }
}