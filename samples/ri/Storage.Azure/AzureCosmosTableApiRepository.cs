namespace Storage.Azure
{
    public class AzureCosmosTableApiRepository : AzureTableStorageRepository
    {
        public AzureCosmosTableApiRepository(string connectionString)
            : base(connectionString, TableStorageApiOptions.AzureCosmosDbStorage)
        {
        }
    }
}