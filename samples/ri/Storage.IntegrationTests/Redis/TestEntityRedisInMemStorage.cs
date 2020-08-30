using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Storage.IntegrationTests.Redis
{
    public class TestEntityRedisInMemCommandStorage<TEntity> : GenericCommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        public TestEntityRedisInMemCommandStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository, string containerName) : base(
            logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }

    public class TestEntityRedisInMemQueryStorage<TEntity> : GenericQueryStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        public TestEntityRedisInMemQueryStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository, string containerName) : base(
            logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}