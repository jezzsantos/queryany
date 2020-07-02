using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    [TestClass, TestCategory("Integration.NOCI")]
    public class AzureCosmosSqlApiStorageSpec : AzureCosmosStorageBaseSpec
    {
        private static AzureCosmosSqlApiRepository repository;
        private readonly Dictionary<string, object> stores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var accountKey = config["AzureCosmosDbAccountKey"];
            var localEmulatorConnectionString = $"AccountEndpoint=https://localhost:8081/;AccountKey={accountKey}";
            repository = new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "TestDatabase",
                new GuidIdentifierFactory());
            InitializeAllTests(context, null);
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
                this.stores.Add(containerName, new TestAzureCosmosStorage<TEntity>(
                    new AzureCosmosConnection(repository), containerName));
            }

            return (IStorage<TEntity>) this.stores[containerName];
        }
    }
}