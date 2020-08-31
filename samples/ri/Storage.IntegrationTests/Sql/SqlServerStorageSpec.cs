using System;
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
        private readonly Dictionary<Type, object> commandStores = new Dictionary<Type, object>();

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("SqlServerServiceName");
            const string databaseName = "TestDatabase";
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

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(IDomainFactory domainFactory)
        {
            if (!this.commandStores.ContainsKey(typeof(TEntity)))
            {
                this.commandStores.Add(typeof(TEntity),
                    new GeneralCommandStorage<TEntity>(Logger, domainFactory,
                        repository));
            }

            return (ICommandStorage<TEntity>) this.commandStores[typeof(TEntity)];
        }
    }

    [TestClass, TestCategory("Integration.Storage")]
    public class SqlServerStorageQuerySpec : AnyQueryStorageBaseSpec
    {
        private static SqlServerRepository repository;
        private readonly Dictionary<Type, object> commandStores = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> queryStores = new Dictionary<Type, object>();

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

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(IDomainFactory domainFactory)
        {
            if (!this.commandStores.ContainsKey(typeof(TEntity)))
            {
                this.commandStores.Add(typeof(TEntity),
                    new GeneralCommandStorage<TEntity>(Logger, domainFactory,
                        repository));
            }

            return (ICommandStorage<TEntity>) this.commandStores[typeof(TEntity)];
        }

        protected override IQueryStorage<TEntity> GetQueryStore<TEntity>(IDomainFactory domainFactory)
        {
            if (!this.queryStores.ContainsKey(typeof(TEntity)))
            {
                this.queryStores.Add(typeof(TEntity),
                    new GeneralQueryStorage<TEntity>(Logger, domainFactory, repository));
            }

            return (IQueryStorage<TEntity>) this.queryStores[typeof(TEntity)];
        }
    }
}