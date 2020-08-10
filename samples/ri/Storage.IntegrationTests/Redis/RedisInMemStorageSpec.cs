using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;
using Storage.Interfaces;
using Storage.Redis;

namespace Storage.IntegrationTests.Redis
{
    [TestClass, TestCategory("Integration")]
    public class RedisInMemStorageSpec : RedisInMemStorageBaseSpec
    {
        private static RedisInMemRepository repository;
        private readonly Dictionary<string, object> stores = new Dictionary<string, object>();

        [ClassInitialize]
        public new static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var localServerConnectionString = config["RedisConnectionString"];
            repository =
                new RedisInMemRepository(localServerConnectionString);
            RedisInMemStorageBaseSpec.InitializeAllTests(context);
        }

        [ClassCleanup]
        public new static void CleanupAllTests()
        {
            RedisInMemStorageBaseSpec.CleanupAllTests();
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName,
            EntityFactory<TEntity> entityFactory)
        {
            if (!this.stores.ContainsKey(containerName))
            {
                this.stores.Add(containerName,
                    new TestEntityInMemStorage<TEntity>(Logger, entityFactory, repository, containerName));
            }

            return (IStorage<TEntity>) this.stores[containerName];
        }

        private class TestEntityInMemStorage<TEntity> : GenericStorage<TEntity>
            where TEntity : IPersistableEntity
        {
            public TestEntityInMemStorage(ILogger logger, EntityFactory<TEntity> entityFactory,
                IRepository repository, string containerName) : base(
                logger, entityFactory, repository)
            {
                containerName.GuardAgainstNullOrEmpty(nameof(containerName));
                ContainerName = containerName;
            }

            protected override string ContainerName { get; }
        }
    }
}