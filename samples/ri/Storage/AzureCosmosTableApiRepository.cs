using Storage.Interfaces;

namespace Storage
{
    public class AzureCosmosTableApiRepository : AzureTableStorageRepository
    {
        public AzureCosmosTableApiRepository(string connectionString, IIdentifierFactory idFactory)
            : base(connectionString, idFactory, TableStorageApiOptions.AzureCosmosDbStorage)
        {
        }
    }
}