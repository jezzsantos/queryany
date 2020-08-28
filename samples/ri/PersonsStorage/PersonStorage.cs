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

        public PersonEntity Load(Identifier id)
        {
            return this.storage.Load<PersonEntity>(id);
        }

        public PersonEntity Save(PersonEntity person)
        {
            this.storage.Save(person);
            return person;
        }

        public PersonEntity FindByEmailAddress(string emailAddress)
        {
            var persons = this.storage.Query(Query.From<PersonEntity>()
                .Where(e => e.Email, ConditionOperator.EqualTo, emailAddress));
            return persons.Results.FirstOrDefault();
        }
    }
}