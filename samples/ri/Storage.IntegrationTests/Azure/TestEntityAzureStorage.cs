using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    public class TestEntityAzureStorage<TEntity> : AzureStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        public TestEntityAzureStorage(ILogger logger, IAzureStorageConnection connection, string containerName) : base(
            logger, connection)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}