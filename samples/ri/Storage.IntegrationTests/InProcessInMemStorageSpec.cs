using Domain.Interfaces.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    [TestClass, TestCategory("Integration.Storage")]
    public class InProcessInMemCommandStorageSpec : AnyCommandStorageBaseSpec
    {
        private readonly InProcessInMemRepository repository;

        public InProcessInMemCommandStorageSpec()
        {
            this.repository = new InProcessInMemRepository();
        }

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            return new TestEntityInMemCommandStorage<TEntity>(Logger, domainFactory, this.repository, containerName);
        }
    }

    [TestClass, TestCategory("Integration.Storage")]
    public class InProcessInMemQueryStorageSpec : AnyQueryStorageBaseSpec
    {
        private readonly InProcessInMemRepository repository;

        public InProcessInMemQueryStorageSpec()
        {
            this.repository = new InProcessInMemRepository();
        }

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            return new TestEntityInMemCommandStorage<TEntity>(Logger, domainFactory, this.repository, containerName);
        }

        protected override IQueryStorage<TEntity> GetQueryStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
        {
            return new TestEntityInMemQueryStorage<TEntity>(Logger, domainFactory, this.repository, containerName);
        }
    }
}