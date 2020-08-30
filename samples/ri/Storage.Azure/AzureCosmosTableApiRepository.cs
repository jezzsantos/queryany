using ServiceStack.Configuration;

namespace Storage.Azure
{
    // ReSharper disable once InconsistentNaming
    public class AzureCosmosTableApiRepository : AzureTableStorageRepository
    {
        public AzureCosmosTableApiRepository(string connectionString)
            : base(connectionString, TableStorageApiOptions.AzureCosmosDbStorage)
        {
        }

        public static AzureCosmosTableApiRepository FromAppSettings(IAppSettings settings)
        {
            var accountKey = settings.GetString("AzureCosmosDbAccountKey");
            var hostName = settings.GetString("AzureCosmosDbHostName");
            var localEmulatorConnectionString =
                $"DefaultEndpointsProtocol=http;AccountName={hostName};AccountKey={accountKey};TableEndpoint=http://localhost:8902/;";
            return new AzureCosmosTableApiRepository(localEmulatorConnectionString);
        }
    }
}