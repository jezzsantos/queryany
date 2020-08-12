using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
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
            var hostName = config["AzureCosmosDbHostName"];
            var localEmulatorConnectionString = $"AccountEndpoint=https://{hostName}:8081/;AccountKey={accountKey}";
            repository = new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "TestDatabase");
            InitializeAllTests(context, null);
        }

        [ClassCleanup]
        public new static void CleanupAllTests()
        {
            AzureCosmosStorageBaseSpec.CleanupAllTests();
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.stores.ContainsKey(containerName))
            {
                this.stores.Add(containerName, new TestEntityAzureStorage<TEntity>(Logger, domainFactory,
                    repository, containerName));
            }

            return (IStorage<TEntity>) this.stores[containerName];
        }
    }
}