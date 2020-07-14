using QueryAny.Primitives;

namespace Storage.Azure
{
    public class AzureStorageConnection : IAzureStorageConnection
    {
        private readonly IRepository repository;

        public AzureStorageConnection(IRepository repository)
        {
            repository.GuardAgainstNull(nameof(repository));
            this.repository = repository;
        }

        public IRepository Open()
        {
            return this.repository;
        }
    }
}