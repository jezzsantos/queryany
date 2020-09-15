using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;

namespace Storage.IntegrationTests
{
    [TestClass, TestCategory("Integration.Storage")]
    public class LocalMachineFileRepositorySpec : AnyRepositoryBaseSpec
    {
        private static LocalMachineFileRepository repository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            repository = LocalMachineFileRepository.FromAppSettings(settings);
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