using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using PersonsDomain;
using QueryAny.Primitives;
using Storage;

namespace PersonsStorage
{
    public class PersonEntityInMemStorage : GenericStorage<PersonEntity>
    {
        private PersonEntityInMemStorage(ILogger logger, IDomainFactory domainFactory,
            InProcessInMemRepository repository) : base(logger, domainFactory, repository)
        {
        }

        protected override string ContainerName => "Person";

        public static PersonEntityInMemStorage Create(ILogger logger, IDomainFactory domainFactory)
        {
            logger.GuardAgainstNull(nameof(logger));

            return new PersonEntityInMemStorage(logger, domainFactory,
                new InProcessInMemRepository());
        }
    }
}