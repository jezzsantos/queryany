using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    [TestClass, TestCategory("Integration")]
    public class AzureTableStorageSpec : AzureStorageAccountBaseSpec
    {
        private static AzureTableStorageRepository repository;
        private readonly Dictionary<string, object> stores = new Dictionary<string, object>();

        [ClassInitialize]
        public new static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var accountKey = config["AzureTableStorageAccountKey"];
            var hostName = config["AzureTableStorageHostName"];
            var localEmulatorConnectionString = accountKey.HasValue()
                ? $"DefaultEndpointsProtocol=https;AccountName={hostName};AccountKey={accountKey};EndpointSuffix=core.windows.net"
                : "UseDevelopmentStorage=true";
            repository = new AzureTableStorageRepository(localEmulatorConnectionString, new GuidIdentifierFactory());
            AzureStorageAccountBaseSpec.InitializeAllTests(context);
        }

        [ClassCleanup]
        public new static void CleanupAllTests()
        {
            AzureStorageAccountBaseSpec.CleanupAllTests();
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName,
            EntityFactory<TEntity> entityFactory)
        {
            if (!this.stores.ContainsKey(containerName))
            {
                this.stores.Add(containerName, new TestEntityAzureStorage<TEntity>(Logger, entityFactory,
                    repository, containerName));
            }

            return (IStorage<TEntity>) this.stores[containerName];
        }
    }
}