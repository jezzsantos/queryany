using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Azure;

namespace Storage.IntegrationTests.Azure
{
    [TestClass, TestCategory("Integration.Storage.NOCI")]

    // ReSharper disable once InconsistentNaming
    public class AzureCosmosSqlApiRepositorySpec : AnyRepositoryBaseSpec
    {
        private static AzureCosmosSqlApiRepository repository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            repository = AzureCosmosSqlApiRepository.FromAppSettings(settings, "TestDatabase");
            AzureCosmosStorageBase.InitializeAllTests(context, null);
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
    }
}