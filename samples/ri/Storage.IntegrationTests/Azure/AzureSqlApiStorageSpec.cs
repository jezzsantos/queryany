using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Interfaces.Entities;
using ServiceStack.Data;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    [TestClass, TestCategory("Integration.NOCI")]
    public class AzureSqlApiStorageSpec : AzureSqlServerStorageBaseSpec
    {
        private static AzureSqlServerRepository repository;
        private readonly Dictionary<string, object> stores = new Dictionary<string, object>();
        private static ISqlConnectionFactory connectionFactory;
        private static string dbName = "QueryAny_IntegrationTests";

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var serverConnectionString = $"Server=localhost,1433;Persist Security Info=False;User ID=sa;Password=@mazingP@ssw0rd;MultipleActiveResultSets=False;Connection Timeout=30;";
            ISqlConnectionFactory connectionFactoryForCreatingDb = new SqlConnectionFactory(serverConnectionString);
            repository = new AzureSqlServerRepository(connectionFactoryForCreatingDb, new GuidIdentifierFactory(), new GuidIdentifierFactory());
            
            // todo (sdv) is there a way I do not have to duplicate connection strings?
            var dbConnectionString = $"Server=localhost,1433;Persist Security Info=False;Initial Catalog={dbName};User ID=sa;Password=@mazingP@ssw0rd;MultipleActiveResultSets=False;Connection Timeout=30;";
            connectionFactory = new SqlConnectionFactory(serverConnectionString);
            InitializeAllTests(context, dbName, connectionFactory);
        }

        [ClassCleanup]
        public new static void CleanupAllTests()
        {
            AzureSqlServerStorageBaseSpec.CleanupAllTests(dbName, connectionFactory);
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName,
            EntityFactory<TEntity> entityFactory)
        {
            if (!this.stores.ContainsKey(containerName))
            {
                // todo (Sdv) Do I need a TestEntitySqlStorage here?
                this.stores.Add(containerName, new TestEntityAzureStorage<TEntity>(Logger, entityFactory,
                    new AzureStorageConnection(repository), containerName));
            }

            return (IStorage<TEntity>) this.stores[containerName];
        }
    }
}