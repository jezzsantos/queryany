using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit.NOCI")]
    public class AzureCosmosSqlApiStorageSpec : AzureCosmosStorageBaseSpec
    {
        private IStorage<TestEntity> storage;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests(context, null);
        }

        [ClassCleanup]
        public new static void CleanupAllTests()
        {
            AzureCosmosStorageBaseSpec.CleanupAllTests();
        }

        protected override IStorage<TestEntity> GetStorage()
        {
            if (this.storage == null)
            {
                var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
                var accountKey = config["AzureCosmosDbAccountKey"];
                var localEmulatorConnectionString = $"AccountEndpoint=https://localhost:8081/;AccountKey={accountKey}";
                this.storage = new TestAzureCosmosStorage(
                    new AzureCosmosConnection(
                        new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "TestDatabase",
                            new GuidIdentifierFactory())));
            }

            return this.storage;
        }
    }
}