using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class InProcessInMemStorageSpec : AnyStorageBaseSpec
    {
        private readonly InProcessInMemRepository repository;

        public InProcessInMemStorageSpec()
        {
            this.repository = new InProcessInMemRepository(new GuidIdentifierFactory());
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName)
        {
            return new TestInMemStorage<TEntity>(
                this.repository, containerName);
        }
    }

    public class TestInMemStorage<TEntity> : InProcessInMemStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        public TestInMemStorage(InProcessInMemRepository store, string containerName) : base(store)
        {
            Guard.AgainstNullOrEmpty(() => containerName, containerName);
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}