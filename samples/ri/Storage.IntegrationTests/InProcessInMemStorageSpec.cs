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

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(IDomainFactory domainFactory)
        {
            return new GeneralCommandStorage<TEntity>(Logger, domainFactory, this.repository);
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

        protected override ICommandStorage<TEntity> GetCommandStore<TEntity>(IDomainFactory domainFactory)
        {
            return new GeneralCommandStorage<TEntity>(Logger, domainFactory,
                this.repository);
        }

        protected override IQueryStorage<TEntity> GetQueryStore<TEntity>(IDomainFactory domainFactory)
        {
            return new GeneralQueryStorage<TEntity>(Logger, domainFactory, this.repository);
        }
    }
}