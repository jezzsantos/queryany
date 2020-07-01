using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    public class TestAzureCosmosStorage<TEntity> : AzureCosmosStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        public TestAzureCosmosStorage(IAzureCosmosConnection connection, string containerName) : base(connection)
        {
            Guard.AgainstNullOrEmpty(() => containerName, containerName);
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}