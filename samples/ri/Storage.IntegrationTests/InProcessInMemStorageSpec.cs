using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    [TestClass, TestCategory("Integration")]
    public class InProcessInMemStorageSpec : AnyStorageBaseSpec
    {
        private readonly InProcessInMemRepository repository;

        public InProcessInMemStorageSpec()
        {
            this.repository = new InProcessInMemRepository(new GuidIdentifierFactory());
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName)
        {
            return new TestEntityInMemStorage<TEntity>(
                this.repository, containerName);
        }

        private class TestEntityInMemStorage<TEntity> : InProcessInMemStorage<TEntity>
            where TEntity : IPersistableEntity, new()
        {
            public TestEntityInMemStorage(InProcessInMemRepository repository, string containerName) : base(repository)
            {
                Guard.AgainstNullOrEmpty(() => containerName, containerName);
                ContainerName = containerName;
            }

            protected override string ContainerName { get; }
        }
    }
}