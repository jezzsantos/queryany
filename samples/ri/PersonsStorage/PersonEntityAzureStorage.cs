using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using PersonsDomain;
using QueryAny.Primitives;
using ServiceStack.Configuration;
using Storage;
using Storage.Azure;

namespace PersonsStorage
{
    public class PersonEntityAzureCommandStorage : GenericCommandStorage<PersonEntity>
    {
        private PersonEntityAzureCommandStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static PersonEntityAzureCommandStorage Create(ILogger logger, IAppSettings settings,
            IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));

            return new PersonEntityAzureCommandStorage(logger, domainFactory,
                AzureCosmosSqlApiRepository.FromAppSettings(settings, "Production"));
        }
    }

    public class PersonEntityAzureQueryStorage : GenericQueryStorage<PersonEntity>
    {
        private PersonEntityAzureQueryStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static PersonEntityAzureQueryStorage Create(ILogger logger, IAppSettings settings,
            IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));

            return new PersonEntityAzureQueryStorage(logger, domainFactory,
                AzureCosmosSqlApiRepository.FromAppSettings(settings, "Production"));
        }
    }
}