using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage;

namespace CarsStorage
{
    public class UnavailabilityEntityInMemCommandStorage : GenericCommandStorage<UnavailabilityEntity>
    {
        private UnavailabilityEntityInMemCommandStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static UnavailabilityEntityInMemCommandStorage Create(ILogger logger, IDomainFactory domainFactory,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));

            return new UnavailabilityEntityInMemCommandStorage(logger, domainFactory, repository);
        }
    }

    public class UnavailabilityEntityInMemQueryStorage : GenericQueryStorage<UnavailabilityEntity>
    {
        private UnavailabilityEntityInMemQueryStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static UnavailabilityEntityInMemQueryStorage Create(ILogger logger, IDomainFactory domainFactory,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));

            return new UnavailabilityEntityInMemQueryStorage(logger, domainFactory, repository);
        }
    }
}