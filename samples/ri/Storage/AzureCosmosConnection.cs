using QueryAny.Primitives;

namespace Storage
{
    public interface IAzureCosmosConnection
    {
        IAzureCosmosRepository Open();
    }

    public class AzureCosmosConnection : IAzureCosmosConnection
    {
        private readonly IAzureCosmosRepository repository;

        public AzureCosmosConnection(IAzureCosmosRepository repository)
        {
            Guard.AgainstNull(() => repository, repository);
            this.repository = repository;
        }

        public IAzureCosmosRepository Open()
        {
            return this.repository;
        }
    }
}