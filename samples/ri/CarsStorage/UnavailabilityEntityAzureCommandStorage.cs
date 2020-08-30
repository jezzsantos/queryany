using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using ServiceStack.Configuration;
using Storage;
using Storage.Azure;

namespace CarsStorage
{
    public class UnavailabilityEntityAzureCommandStorage : GenericCommandStorage<UnavailabilityEntity>
    {
        private UnavailabilityEntityAzureCommandStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static UnavailabilityEntityAzureCommandStorage Create(ILogger logger, IAppSettings settings,
            IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));
            return new UnavailabilityEntityAzureCommandStorage(logger,
                domainFactory,
                AzureCosmosSqlApiRepository.FromAppSettings(settings, "Production"));
        }
    }

    public class UnavailabilityEntityAzureQueryStorage : GenericQueryStorage<UnavailabilityEntity>
    {
        private UnavailabilityEntityAzureQueryStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static UnavailabilityEntityAzureQueryStorage Create(ILogger logger, IAppSettings settings,
            IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));
            return new UnavailabilityEntityAzureQueryStorage(logger,
                domainFactory,
                AzureCosmosSqlApiRepository.FromAppSettings(settings, "Production"));
        }
    }
}