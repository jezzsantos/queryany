using CarsDomain.Entities;
using Microsoft.Extensions.Logging;

namespace Storage
{
    public class CarEntityInMemStorage : GenericStorage<CarEntity>
    {
        public CarEntityInMemStorage(ILogger logger, InProcessInMemRepository repository) : base(logger, repository)
        {
        }

        protected override string ContainerName => "Car";
    }
}