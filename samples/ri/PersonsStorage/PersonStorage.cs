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
        private readonly ICommandStorage<PersonEntity> commandStorage;
        private readonly IQueryStorage<PersonEntity> queryStorage;

        public PersonStorage(ICommandStorage<PersonEntity> commandStorage, IQueryStorage<PersonEntity> queryStorage)
        {
            commandStorage.GuardAgainstNull(nameof(commandStorage));
            queryStorage.GuardAgainstNull(nameof(queryStorage));
            this.commandStorage = commandStorage;
            this.queryStorage = queryStorage;
        }

        public PersonEntity Load(Identifier id)
        {
            return this.commandStorage.Load<PersonEntity>(id);
        }

        public PersonEntity Save(PersonEntity person)
        {
            this.commandStorage.Save(person);
            return person;
        }

        public PersonEntity FindByEmailAddress(string emailAddress)
        {
            var persons = this.queryStorage.Query(Query.From<PersonEntity>()
                .Where(e => e.Email, ConditionOperator.EqualTo, emailAddress));
            return persons.Results.FirstOrDefault();
        }
    }
}