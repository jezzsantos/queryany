using System;
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
        private readonly Dictionary<Type, object> commandStores = new Dictionary<Type, object>();

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
    public class RedisInMemQueryStorageSpec : AnyQueryStorageBaseSpec
    {
        private static RedisInMemRepository repository;
        private readonly Dictionary<Type, object> commandStores = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> queryStores = new Dictionary<Type, object>();

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