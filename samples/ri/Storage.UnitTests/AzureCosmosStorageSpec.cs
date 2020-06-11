using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass]
    public class AzureCosmosStorageSpec : StorageSpecBase
    {
        protected override IStorage<TestEntity> GetStorage()
        {
            return new TestAzureCosmosStorage();
        }
    }

    public class TestAzureCosmosStorage : AzureCosmosStorage<TestEntity>
    {
    }
}