using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using ServiceStack.Configuration;
using Storage;
using Storage.Azure;

namespace CarsStorage
{
    public class CarEntityAzureStorage : GenericStorage<CarEntity>
    {
        private CarEntityAzureStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        protected override string ContainerName => "Car";

        public static CarEntityAzureStorage Create(ILogger logger, IAppSettings settings, IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));
            var accountKey = settings.GetString("AzureCosmosDbAccountKey");
            var hostName = settings.GetString("AzureCosmosDbHostName");
            var localEmulatorConnectionString = $"AccountEndpoint=https://{hostName}:8081/;AccountKey={accountKey}";

            return new CarEntityAzureStorage(logger,
                domainFactory,
                new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "Production"));
        }
    }
}