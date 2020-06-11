using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass]
    public class InMemStorageSpec : StorageSpecBase
    {
        protected override IStorage<TestEntity> GetStorage()
        {
            return new TestInMemStorage();
        }
    }

    public class TestInMemStorage : InMemStorage<TestEntity>
    {
    }
}