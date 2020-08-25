using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage;

namespace CarsStorage
{
    public class UnavailabilityEntityInMemStorage : GenericStorage<UnavailabilityEntity>
    {
        private UnavailabilityEntityInMemStorage(ILogger logger, IDomainFactory domainFactory,
            InProcessInMemRepository repository) : base(logger, domainFactory, repository)
        {
        }

        protected override string ContainerName => "Unavailability";

        public static UnavailabilityEntityInMemStorage Create(ILogger logger, IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));

            return new UnavailabilityEntityInMemStorage(logger, domainFactory,
                new InProcessInMemRepository());
        }
    }
}