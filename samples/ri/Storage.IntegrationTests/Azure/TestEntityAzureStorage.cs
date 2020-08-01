using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Services.Interfaces.Entities;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    public class TestEntityAzureStorage<TEntity> : GenericStorage<TEntity> where TEntity : IPersistableEntity
    {
        public TestEntityAzureStorage(ILogger logger, EntityFactory<TEntity> entityFactory,
            IRepository repository, string containerName) : base(
            logger, entityFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}