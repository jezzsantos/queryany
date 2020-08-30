using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    [TestClass, TestCategory("Integration.Storage")]

    // ReSharper disable once InconsistentNaming
    public class AzureTableCommandStorageSpec : AnyCommandStorageBaseSpec
    {
        private static AzureTableStorageRepository repository;
        private readonly Dictionary<string, object> commandStores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
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

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.commandStores.ContainsKey(containerName))
            {
                this.commandStores.Add(containerName, new TestEntityAzureCommandStorage<TEntity>(Logger, domainFactory,
                    repository, containerName));
            }

            return (ICommandStorage<TEntity>) this.commandStores[containerName];
        }
    }

    [TestClass, TestCategory("Integration.Storage")]

    // ReSharper disable once InconsistentNaming
    public class AzureTableQueryStorageSpec : AnyQueryStorageBaseSpec
    {
        private static AzureTableStorageRepository repository;
        private readonly Dictionary<string, object> commandStores = new Dictionary<string, object>();
        private readonly Dictionary<string, object> queryStores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
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

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.commandStores.ContainsKey(containerName))
            {
                this.commandStores.Add(containerName, new TestEntityAzureCommandStorage<TEntity>(Logger, domainFactory,
                    repository, containerName));
            }

            return (ICommandStorage<TEntity>) this.commandStores[containerName];
        }

        protected override IQueryStorage<TEntity> GetQueryStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.queryStores.ContainsKey(containerName))
            {
                this.queryStores.Add(containerName, new TestEntityAzureQueryStorage<TEntity>(Logger, domainFactory,
                    repository, containerName));
            }

            return (IQueryStorage<TEntity>) this.queryStores[containerName];
        }
    }
}