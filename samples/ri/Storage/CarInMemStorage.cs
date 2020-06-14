using CarsDomain.Entities;

namespace Storage
{
    public class CarInMemStorage : InMemStorage<CarEntity>
    {
        public CarInMemStorage(InMemRepository repository) : base(repository)
        {
        }

        protected override string ContainerName => "Car";
    }
}