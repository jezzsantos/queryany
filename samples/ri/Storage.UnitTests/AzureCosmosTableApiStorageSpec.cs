using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class AzureCosmosTableApiStorageSpec : AzureCosmosStorageBaseSpec
    {
        private IStorage<TestEntity> storage;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests(context, "/EnableTableEndpoint");
        }

        [ClassCleanup]
        public new static void CleanupAllTests()
        {
            AzureCosmosStorageBaseSpec.CleanupAllTests();
        }

        protected override IStorage<TestEntity> GetStorage()
        {
            if (this.storage == null)
            {
                var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
                var accountKey = config["AzureCosmosDbAccountKey"];
                var localEmulatorConnectionString =
                    $"DefaultEndpointsProtocol=http;AccountName=localhost;AccountKey={accountKey};TableEndpoint=http://localhost:8902/;";
                this.storage = new TestAzureCosmosStorage(
                    new AzureCosmosConnection(
                        new AzureCosmosTableApiRepository(localEmulatorConnectionString, new GuidIdentifierFactory())));
            }

            return this.storage;
        }
    }
}