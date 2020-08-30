using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using ServiceStack.Configuration;
using Storage;
using Storage.Azure;

namespace CarsStorage
{
    public class CarEntityAzureCommandStorage : GenericCommandStorage<CarEntity>
    {
        private CarEntityAzureCommandStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static CarEntityAzureCommandStorage Create(ILogger logger, IAppSettings settings,
            IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));

            return new CarEntityAzureCommandStorage(logger, domainFactory,
                AzureCosmosSqlApiRepository.FromAppSettings(settings, "Production"));
        }
    }

    public class CarEntityAzureQueryStorage : GenericQueryStorage<CarEntity>
    {
        private CarEntityAzureQueryStorage(ILogger logger,
            IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static CarEntityAzureQueryStorage Create(ILogger logger, IAppSettings settings,
            IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            settings.GuardAgainstNull(nameof(settings));

            return new CarEntityAzureQueryStorage(logger, domainFactory,
                AzureCosmosSqlApiRepository.FromAppSettings(settings, "Production"));
        }
    }
}