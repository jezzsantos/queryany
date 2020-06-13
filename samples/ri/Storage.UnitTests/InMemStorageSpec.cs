using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass]
    public class InMemStorageSpec : StorageSpecBase
    {
        protected override IStorage<TestEntity> GetStorage()
        {
            return new TestInMemStorage(new InMemEntityRepository(new GuidIdentifierFactory()));
        }
    }

    public class TestInMemStorage : InMemStorage<TestEntity>
    {
        public TestInMemStorage(InMemEntityRepository store) : base(store)
        {
        }

        protected override string EntityName => "test";
    }
}