using System.Linq;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using PersonsApplication.Storage;
using PersonsDomain;
using QueryAny;
using QueryAny.Primitives;
using Storage;
using Storage.Interfaces;

namespace PersonsStorage
{
    public class PersonStorage : IPersonStorage
    {
        private readonly IEventingStorage<PersonEntity> eventingStorage;
        private readonly IQueryStorage<PersonEntity> queryStorage;

        public PersonStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            this.queryStorage = new GeneralQueryStorage<PersonEntity>(logger, domainFactory, repository);
            this.eventingStorage = new GeneralEventingStorage<PersonEntity>(logger, domainFactory, repository);
        }

        public PersonStorage(IEventingStorage<PersonEntity> eventingStorage, IQueryStorage<PersonEntity> queryStorage)
        {
            queryStorage.GuardAgainstNull(nameof(queryStorage));
            eventingStorage.GuardAgainstNull(nameof(eventingStorage));
            this.queryStorage = queryStorage;
            this.eventingStorage = eventingStorage;
        }

        public PersonEntity Load(Identifier id)
        {
            return this.eventingStorage.Load(id);
        }

        public PersonEntity Save(PersonEntity person)
        {
            this.eventingStorage.Save(person);
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