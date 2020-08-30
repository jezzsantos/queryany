using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack;
using Storage.Interfaces;
using Storage.Sql;

namespace Storage.IntegrationTests.Sql
{
    [TestClass, TestCategory("Integration.Storage")]
    public class SqlServerStorageCommandSpec : AnyCommandStorageBaseSpec
    {
        private static SqlServerRepository repository;
        private readonly Dictionary<string, object> commandStores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("SqlServerServiceName");
            var databaseName = "TestDatabase";
            repository = SqlServerRepository.FromSettings(settings, databaseName);
            SqlServerStorageBase.InitializeAllTests(context, serviceName, databaseName);
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("SqlServerServiceName");
            SqlServerStorageBase.CleanupAllTests(serviceName);
        }

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.commandStores.ContainsKey(containerName))
            {
                this.commandStores.Add(containerName,
                    new TestEntitySqlCommandStorage<TEntity>(Logger, domainFactory, repository, containerName));
            }

            return (ICommandStorage<TEntity>) this.commandStores[containerName];
        }
    }

    [TestClass, TestCategory("Integration.Storage")]
    public class SqlServerStorageQuerySpec : AnyQueryStorageBaseSpec
    {
        private static SqlServerRepository repository;
        private readonly Dictionary<string, object> commandStores = new Dictionary<string, object>();
        private readonly Dictionary<string, object> queryStores = new Dictionary<string, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("SqlServerServiceName");
            var databaseName = "TestDatabase";
            repository = SqlServerRepository.FromSettings(settings, databaseName);
            SqlServerStorageBase.InitializeAllTests(context, serviceName, databaseName);
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("SqlServerServiceName");
            SqlServerStorageBase.CleanupAllTests(serviceName);
        }

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.commandStores.ContainsKey(containerName))
            {
                this.commandStores.Add(containerName,
                    new TestEntitySqlCommandStorage<TEntity>(Logger, domainFactory, repository, containerName));
            }

            return (ICommandStorage<TEntity>) this.commandStores[containerName];
        }

        protected override IQueryStorage<TEntity> GetQueryStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            if (!this.queryStores.ContainsKey(containerName))
            {
                this.queryStores.Add(containerName,
                    new TestEntitySqlQueryStorage<TEntity>(Logger, domainFactory, repository, containerName));
            }

            return (IQueryStorage<TEntity>) this.queryStores[containerName];
        }
    }
}