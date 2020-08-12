using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage;

namespace CarsStorage
{
    public class CarEntityInMemStorage : GenericStorage<CarEntity>
    {
        private CarEntityInMemStorage(ILogger logger, IDomainFactory domainFactory,
            InProcessInMemRepository repository) : base(logger, domainFactory, repository)
        {
        }

        protected override string ContainerName => "Car";

        public static CarEntityInMemStorage Create(ILogger logger, IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));

            return new CarEntityInMemStorage(logger, domainFactory,
                new InProcessInMemRepository());
        }
    }
}