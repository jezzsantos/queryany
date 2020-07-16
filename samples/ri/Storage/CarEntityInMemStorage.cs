using CarsDomain.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage
{
    public class CarEntityInMemStorage : GenericStorage<CarEntity>
    {
        private CarEntityInMemStorage(ILogger logger, EntityFactory<CarEntity> entityFactory,
            InProcessInMemRepository repository) : base(logger, entityFactory, repository)
        {
        }

        protected override string ContainerName => "Car";

        public static CarEntityInMemStorage Create(ILogger logger, GuidIdentifierFactory identifierFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            identifierFactory.GuardAgainstNull(nameof(identifierFactory));
            return new CarEntityInMemStorage(logger, CarEntity.GetFactory(logger),
                new InProcessInMemRepository(identifierFactory));
        }
    }
}