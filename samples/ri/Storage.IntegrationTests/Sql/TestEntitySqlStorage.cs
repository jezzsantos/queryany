using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Storage.IntegrationTests.Sql
{
    public class TestEntitySqlCommandStorage<TEntity> : GenericCommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        public TestEntitySqlCommandStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository, string containerName) : base(
            logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }

    public class TestEntitySqlQueryStorage<TEntity> : GenericQueryStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        public TestEntitySqlQueryStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository, string containerName) : base(
            logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}