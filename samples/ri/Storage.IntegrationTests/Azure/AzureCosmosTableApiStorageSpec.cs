using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    [TestClass, TestCategory("Integration")]
    public class AzureCosmosTableApiStorageSpec : AzureCosmosStorageBaseSpec
    {
        private static AzureCosmosTableApiRepository repository;
        private readonly Dictionary<string, object> stores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var accountKey = config["AzureCosmosDbAccountKey"];
            var hostName = config["AzureCosmosDbHostName"];
            var localEmulatorConnectionString =
                $"DefaultEndpointsProtocol=http;AccountName={hostName};AccountKey={accountKey};TableEndpoint=http://localhost:8902/;";
            repository = new AzureCosmosTableApiRepository(localEmulatorConnectionString, new GuidIdentifierFactory());
            InitializeAllTests(context, "/EnableTableEndpoint");
        }

        [ClassCleanup]
        public new static void CleanupAllTests()
        {
            AzureCosmosStorageBaseSpec.CleanupAllTests();
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName)
        {
            if (!this.stores.ContainsKey(containerName))
            {
                this.stores.Add(containerName, new TestEntityAzureStorage<TEntity>(Logger,
                    new AzureStorageConnection(repository), containerName));
            }

            return (IStorage<TEntity>) this.stores[containerName];
        }
    }
}