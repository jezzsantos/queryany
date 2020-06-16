using CarsDomain.Entities;

namespace Storage
{
    public class CarInMemStorage : InProcessInMemStorage<CarEntity>
    {
        public CarInMemStorage(InProcessInMemRepository repository) : base(repository)
        {
        }

        protected override string ContainerName => "Car";
    }
}