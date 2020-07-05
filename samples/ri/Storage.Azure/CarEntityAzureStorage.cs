using CarsDomain.Entities;

namespace Storage.Azure
{
    public class CarEntityAzureStorage : AzureStorage<CarEntity>
    {
        public CarEntityAzureStorage(AzureStorageConnection connection) : base(connection)
        {
        }

        protected override string ContainerName => "Car";
    }
}