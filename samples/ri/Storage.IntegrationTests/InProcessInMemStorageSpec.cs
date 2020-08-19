using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    [TestClass, TestCategory("Integration.Storage")]
    public class InProcessInMemStorageSpec : AnyStorageBaseSpec
    {
        private readonly InProcessInMemRepository repository;

        public InProcessInMemStorageSpec()
        {
            this.repository = new InProcessInMemRepository();
        }

        protected override IStorage<TEntity> GetStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            return new TestEntityInMemStorage<TEntity>(Logger, domainFactory, this.repository, containerName);
        }

        private class TestEntityInMemStorage<TEntity> : GenericStorage<TEntity>
            where TEntity : IPersistableEntity
        {
            public TestEntityInMemStorage(ILogger logger, IDomainFactory domainFactory,
                InProcessInMemRepository repository, string containerName) :
                base(logger, domainFactory, repository)
            {
                containerName.GuardAgainstNullOrEmpty(nameof(containerName));
                ContainerName = containerName;
            }

            protected override string ContainerName { get; }
        }
    }
}