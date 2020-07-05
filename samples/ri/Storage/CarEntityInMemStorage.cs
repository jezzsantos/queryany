using CarsDomain.Entities;

namespace Storage
{
    public class CarEntityInMemStorage : InProcessInMemStorage<CarEntity>
    {
        public CarEntityInMemStorage(InProcessInMemRepository repository) : base(repository)
        {
        }

        protected override string ContainerName => "Car";
    }
}