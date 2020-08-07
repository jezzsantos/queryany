using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;
using Storage.Interfaces;
using Storage.Sql;

namespace Storage.IntegrationTests.Sql
{
    [TestClass, TestCategory("Integration")]
    public class SqlServerStorageSpec : SqlServerStorageBaseSpec
    {
        private static SqlServerRepository repository;
        private readonly Dictionary<string, object> stores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var serverName = config["SqlServerDbServerName"];
            var credentials = config["SqlServerDbCredentials"];
            var serviceName = config["SqlServerServiceName"];
            var databaseName = "TestDatabase";
            repository =
                new SqlServerRepository(
                    $"Persist Security Info=False;Integrated Security=true;Initial Catalog={databaseName};Server={serverName}{(credentials.HasValue() ? ";" + credentials : "")}",
                    new GuidIdentifierFactory());
            InitializeAllTests(context, serviceName, databaseName);
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var serviceName = config["SqlServerServiceName"];
            CleanupAllTests(serviceName);
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName,
            EntityFactory<TEntity> entityFactory)
        {
            if (!this.stores.ContainsKey(containerName))
            {
                this.stores.Add(containerName,
                    new TestEntitySqlStorage<TEntity>(Logger, entityFactory, repository, containerName));
            }

            return (IStorage<TEntity>) this.stores[containerName];
        }

        private class TestEntitySqlStorage<TEntity> : GenericStorage<TEntity>
            where TEntity : IPersistableEntity
        {
            public TestEntitySqlStorage(ILogger logger, EntityFactory<TEntity> entityFactory,
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