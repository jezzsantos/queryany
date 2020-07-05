using QueryAny.Primitives;

namespace Storage
{
    public interface IAzureStorageConnection
    {
        IRepository Open();
    }

    public class AzureStorageConnection : IAzureStorageConnection
    {
        private readonly IRepository repository;

        public AzureStorageConnection(IRepository repository)
        {
            Guard.AgainstNull(() => repository, repository);
            this.repository = repository;
        }

        public IRepository Open()
        {
            return this.repository;
        }
    }
}