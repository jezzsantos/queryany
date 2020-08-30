using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Storage.IntegrationTests
{
    public class TestEntityInMemCommandStorage<TEntity> : GenericCommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        public TestEntityInMemCommandStorage(ILogger logger, IDomainFactory domainFactory,
            InProcessInMemRepository repository, string containerName) :
            base(logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }

    public class TestEntityInMemQueryStorage<TEntity> : GenericQueryStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        public TestEntityInMemQueryStorage(ILogger logger, IDomainFactory domainFactory,
            InProcessInMemRepository repository, string containerName) :
            base(logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}