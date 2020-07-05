using QueryAny.Primitives;
using Storage.Azure;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    public class TestAzureStorage<TEntity> : AzureStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        public TestAzureStorage(IAzureStorageConnection connection, string containerName) : base(connection)
        {
            Guard.AgainstNullOrEmpty(() => containerName, containerName);
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}