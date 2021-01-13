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
            repository = LocalMachineFileRepository.FromSettings(settings);
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

    [TestClass, TestCategory("Integration.Storage")]
    public class LocalMachineFileBlobositorySpec : AnyBlobositoryBaseSpec
    {
        private static LocalMachineFileRepository blobository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            blobository = LocalMachineFileRepository.FromSettings(settings);
        }

        protected override BloboInfo GetBlobository<TEntity>()
        {
            return new BloboInfo
            {
                Blobository = blobository,
                ContainerName = typeof(TEntity).GetEntityNameSafe()
            };
        }
    }
}