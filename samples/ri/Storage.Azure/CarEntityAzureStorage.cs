using CarsDomain.Entities;
using Microsoft.Extensions.Logging;

namespace Storage.Azure
{
    public class CarEntityAzureStorage : AzureStorage<CarEntity>
    {
        public CarEntityAzureStorage(ILogger logger, AzureStorageConnection connection) : base(logger, connection)
        {
        }

        protected override string ContainerName => "Car";
    }
}