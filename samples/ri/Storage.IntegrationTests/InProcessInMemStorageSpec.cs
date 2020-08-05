using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
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

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName,
            EntityFactory<TEntity> entityFactory)
        {
            return new TestEntityInMemStorage<TEntity>(Logger, entityFactory, this.repository, containerName);
        }

        private class TestEntityInMemStorage<TEntity> : GenericStorage<TEntity>
            where TEntity : IPersistableEntity
        {
            public TestEntityInMemStorage(ILogger logger, EntityFactory<TEntity> entityFactory,
                InProcessInMemRepository repository, string containerName) :
                base(logger, entityFactory, repository)
            {
                containerName.GuardAgainstNullOrEmpty(nameof(containerName));
                ContainerName = containerName;
            }

            protected override string ContainerName { get; }
        }
    }
}