using QueryAny.Primitives;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    public class TestEntityAzureStorage<TEntity> : AzureStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        public TestEntityAzureStorage(IAzureStorageConnection connection, string containerName) : base(connection)
        {
            Guard.AgainstNullOrEmpty(() => containerName, containerName);
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}