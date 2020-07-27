using CarsDomain.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using ServiceStack.Configuration;
using Storage.Interfaces;

namespace Storage.Azure
{
    public class CarEntityAzureStorage : GenericStorage<CarEntity>
    {
        private CarEntityAzureStorage(ILogger logger,
            EntityFactory<CarEntity> entityFactory,
            IRepository repository) : base(logger, entityFactory, repository)
        {
        }

        protected override string ContainerName => "Car";

        public static CarEntityAzureStorage Create(ILogger logger, IAppSettings settings,
            IIdentifierFactory identifierFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));
            var accountKey = settings.GetString("AzureCosmosDbAccountKey");
            var hostName = settings.GetString("AzureCosmosDbHostName");
            var localEmulatorConnectionString = $"AccountEndpoint=https://{hostName}:8081/;AccountKey={accountKey}";

            return new CarEntityAzureStorage(logger,
                CarEntity.GetFactory(logger),
                new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "Production",
                    identifierFactory));
        }
    }
}