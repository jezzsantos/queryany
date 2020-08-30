using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using PersonsDomain;
using QueryAny.Primitives;
using Storage;

namespace PersonsStorage
{
    public class PersonEntityInMemCommandStorage : GenericCommandStorage<PersonEntity>
    {
        private PersonEntityInMemCommandStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static PersonEntityInMemCommandStorage Create(ILogger logger, IDomainFactory domainFactory,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));

            return new PersonEntityInMemCommandStorage(logger, domainFactory, repository);
        }
    }

    public class PersonEntityInMemQueryStorage : GenericQueryStorage<PersonEntity>
    {
        private PersonEntityInMemQueryStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository) : base(logger, domainFactory, repository)
        {
        }

        public static PersonEntityInMemQueryStorage Create(ILogger logger, IDomainFactory domainFactory,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));

            return new PersonEntityInMemQueryStorage(logger, domainFactory, repository);
        }
    }
}