using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using ServiceStack.Configuration;
using Storage;
using Storage.Azure;

namespace CarsStorage
{
    public class UnavailabilityEntityAzureStorage : GenericStorage<UnavailabilityEntity>
    {
        private UnavailabilityEntityAzureStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        protected override string ContainerName => "Unavailability";

        public static UnavailabilityEntityAzureStorage Create(ILogger logger, IAppSettings settings,
            IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));
            var accountKey = settings.GetString("AzureCosmosDbAccountKey");
            var hostName = settings.GetString("AzureCosmosDbHostName");
            var localEmulatorConnectionString = $"AccountEndpoint=https://{hostName}:8081/;AccountKey={accountKey}";

            return new UnavailabilityEntityAzureStorage(logger,
                domainFactory,
                new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "Production"));
        }
    }
}