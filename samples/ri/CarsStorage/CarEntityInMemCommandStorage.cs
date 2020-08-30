using CarsDomain;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage;

namespace CarsStorage
{
    public class CarEntityInMemCommandStorage : GenericCommandStorage<CarEntity>
    {
        private CarEntityInMemCommandStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static CarEntityInMemCommandStorage Create(ILogger logger, IDomainFactory domainFactory,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));

            return new CarEntityInMemCommandStorage(logger, domainFactory, repository);
        }
    }

    public class CarEntityInMemQueryStorage : GenericQueryStorage<CarEntity>
    {
        private CarEntityInMemQueryStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static CarEntityInMemQueryStorage Create(ILogger logger, IDomainFactory domainFactory,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));

            return new CarEntityInMemQueryStorage(logger, domainFactory, repository);
        }
    }
}