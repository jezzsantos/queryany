using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass]
    public class InMemStorageSpec : StorageSpecBase
    {
        protected override IStorage<TestEntity> GetStorage()
        {
            return new TestInMemStorage(new InMemRepository(new GuidIdentifierFactory()));
        }
    }

    public class TestInMemStorage : InMemStorage<TestEntity>
    {
        public TestInMemStorage(InMemRepository store) : base(store)
        {
        }

        protected override string ContainerName => "test";
    }
}