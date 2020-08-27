using System.Linq;
using Domain.Interfaces.Entities;
using PersonsApplication.Storage;
using PersonsDomain;
using QueryAny;
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

        public PersonEntity FindByEmailAddress(string emailAddress)
        {
            var persons = this.storage.Query(Query.From<PersonEntity>()
                .Where(e => e.Email, ConditionOperator.EqualTo, emailAddress));
            return persons.Results.FirstOrDefault();
        }
    }
}