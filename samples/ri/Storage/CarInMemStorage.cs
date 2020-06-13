using CarsDomain.Entities;

namespace Storage
{
    public class CarInMemStorage : InMemStorage<CarEntity>
    {
        public CarInMemStorage(InMemEntityRepository repository) : base(repository)
        {
        }

        protected override string EntityName => "Car";
    }
}