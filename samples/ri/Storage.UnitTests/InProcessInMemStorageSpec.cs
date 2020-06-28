using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class InProcessInMemStorageSpec : AnyStorageBaseSpec
    {
        protected override IStorage<TestEntity> GetStorage()
        {
            return new TestInMemStorage(new InProcessInMemRepository(new GuidIdentifierFactory()));
        }
    }

    public class TestInMemStorage : InProcessInMemStorage<TestEntity>
    {
        public TestInMemStorage(InProcessInMemRepository store) : base(store)
        {
        }

        protected override string ContainerName => "test";
    }
}