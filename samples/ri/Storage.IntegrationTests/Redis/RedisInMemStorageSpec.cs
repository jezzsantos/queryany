using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;
using Storage.Interfaces;
using Storage.Redis;

namespace Storage.IntegrationTests.Redis
{
    [TestClass, TestCategory("Integration.Storage")]
    public class RedisInMemCommandStorageSpec : AnyCommandStorageBaseSpec
    {
        private static RedisInMemRepository repository;
        private readonly Dictionary<string, object> commandStores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            repository = RedisInMemRepository.FromAppSettings(settings);
            RedisInMemStorageBase.InitializeAllTests(context);
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            RedisInMemStorageBase.CleanupAllTests();
        }

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.commandStores.ContainsKey(containerName))
            {
                this.commandStores.Add(containerName,
                    new TestEntityRedisInMemCommandStorage<TEntity>(Logger, domainFactory, repository, containerName));
            }

            return (ICommandStorage<TEntity>) this.commandStores[containerName];
        }
    }

    [TestClass, TestCategory("Integration.Storage")]
    public class RedisInMemQueryStorageSpec : AnyQueryStorageBaseSpec
    {
        private static RedisInMemRepository repository;
        private readonly Dictionary<string, object> commandStores = new Dictionary<string, object>();
        private readonly Dictionary<string, object> queryStores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            repository = RedisInMemRepository.FromAppSettings(settings);
            RedisInMemStorageBase.InitializeAllTests(context);
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            RedisInMemStorageBase.CleanupAllTests();
        }

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.commandStores.ContainsKey(containerName))
            {
                this.commandStores.Add(containerName,
                    new TestEntityRedisInMemCommandStorage<TEntity>(Logger, domainFactory, repository, containerName));
            }

            return (ICommandStorage<TEntity>) this.commandStores[containerName];
        }

        protected override IQueryStorage<TEntity> GetQueryStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.queryStores.ContainsKey(containerName))
            {
                this.queryStores.Add(containerName,
                    new TestEntityRedisInMemQueryStorage<TEntity>(Logger, domainFactory, repository, containerName));
            }

            return (IQueryStorage<TEntity>) this.queryStores[containerName];
        }
    }
}