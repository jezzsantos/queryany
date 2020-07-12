using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
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
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var localServerConnectionString = config["RedisConnectionString"];
            repository =
                new RedisInMemRepository(localServerConnectionString, new GuidIdentifierFactory());
            InitializeAllTests(context, null);
        }

        [ClassCleanup]
        public new static void CleanupAllTests()
        {
            RedisInMemStorageBaseSpec.CleanupAllTests();
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName)
        {
            if (!this.stores.ContainsKey(containerName))
            {
                this.stores.Add(containerName, new TestEntityInMemStorage<TEntity>(
                    repository, containerName));
            }

            return (IStorage<TEntity>) this.stores[containerName];
        }

        private class TestEntityInMemStorage<TEntity> : RedisInMemStorage<TEntity>
            where TEntity : IPersistableEntity, new()
        {
            public TestEntityInMemStorage(RedisInMemRepository repository, string containerName) : base(repository)
            {
                Guard.AgainstNullOrEmpty(() => containerName, containerName);
                ContainerName = containerName;
            }

            protected override string ContainerName { get; }
        }
    }
}