using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Azure;

namespace Storage.IntegrationTests.Azure
{
    [TestClass, TestCategory("Integration.Storage")]

    // ReSharper disable once InconsistentNaming
    public class AzureTableRepositorySpec : AnyRepositoryBaseSpec
    {
        private static AzureTableStorageRepository repository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            repository = AzureTableStorageRepository.FromSettings(settings);
            AzureStorageAccountBase.InitializeAllTests(context);
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            AzureStorageAccountBase.CleanupAllTests();
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