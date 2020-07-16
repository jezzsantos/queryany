using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    public class TestEntityAzureStorage<TEntity> : AzureStorage<TEntity> where TEntity : IPersistableEntity
    {
        public TestEntityAzureStorage(ILogger logger, EntityFactory<TEntity> entityFactory,
            IAzureStorageConnection connection, string containerName) : base(
            logger, entityFactory, connection)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}