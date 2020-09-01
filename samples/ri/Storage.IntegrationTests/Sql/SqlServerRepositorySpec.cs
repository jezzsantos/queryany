using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Sql;

namespace Storage.IntegrationTests.Sql
{
    [TestClass, TestCategory("Integration.Storage")]
    public class SqlServerRepositorySpec : AnyRepositoryBaseSpec
    {
        private static SqlServerRepository repository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("SqlServerServiceName");
            const string databaseName = "TestDatabase";
            repository = SqlServerRepository.FromSettings(settings, databaseName);
            SqlServerStorageBase.InitializeAllTests(context, serviceName, databaseName);
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("SqlServerServiceName");
            SqlServerStorageBase.CleanupAllTests(serviceName);
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