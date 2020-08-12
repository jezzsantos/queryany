using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Storage.IntegrationTests.Azure
{
    public class TestEntityAzureStorage<TEntity> : GenericStorage<TEntity> where TEntity : IPersistableEntity
    {
        public TestEntityAzureStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository, string containerName) : base(
            logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}