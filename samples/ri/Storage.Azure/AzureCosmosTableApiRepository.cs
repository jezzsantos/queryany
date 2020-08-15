namespace Storage.Azure
{
    // ReSharper disable once InconsistentNaming
    public class AzureCosmosTableApiRepository : AzureTableStorageRepository
    {
        public AzureCosmosTableApiRepository(string connectionString)
            : base(connectionString, TableStorageApiOptions.AzureCosmosDbStorage)
        {
        }
    }
}