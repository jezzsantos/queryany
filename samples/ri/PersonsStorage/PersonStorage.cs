using Domain.Interfaces.Entities;
using PersonsApplication.Storage;
using PersonsDomain;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace PersonsStorage
{
    public class PersonStorage : IPersonStorage
    {
        private readonly IStorage<PersonEntity> storage;

        public PersonStorage(IStorage<PersonEntity> storage)
        {
            storage.GuardAgainstNull(nameof(storage));
            this.storage = storage;
        }

        public PersonEntity Get(Identifier id)
        {
            return this.storage.Get(id);
        }

        public PersonEntity Create(PersonEntity person)
        {
            return this.storage.Add(person);
        }
    }
}