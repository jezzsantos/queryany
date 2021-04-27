using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Azure;

namespace Storage.IntegrationTests.Azure
{
    [TestClass, TestCategory("Integration.Storage")]
    public class AzureBlobBlobositorySpec : AnyBlobositoryBaseSpec
    {
        private static AzureBlobStorageRepository blobository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            blobository = AzureBlobStorageRepository.FromSettings(settings);
            AzureStorageAccountBase.InitializeAllTests();
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            AzureStorageAccountBase.CleanupAllTests();
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