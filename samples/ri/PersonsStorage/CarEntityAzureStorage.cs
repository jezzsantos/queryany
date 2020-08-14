using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using PersonsDomain;
using QueryAny.Primitives;
using ServiceStack.Configuration;
using Storage;
using Storage.Azure;

namespace PersonsStorage
{
    public class PersonEntityAzureStorage : GenericStorage<PersonEntity>
    {
        private PersonEntityAzureStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        protected override string ContainerName => "Person";

        public static PersonEntityAzureStorage Create(ILogger logger, IAppSettings settings,
            IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));
            var accountKey = settings.GetString("AzureCosmosDbAccountKey");
            var hostName = settings.GetString("AzureCosmosDbHostName");
            var localEmulatorConnectionString = $"AccountEndpoint=https://{hostName}:8081/;AccountKey={accountKey}";

            return new PersonEntityAzureStorage(logger,
                domainFactory,
                new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "Production"));
        }
    }
}