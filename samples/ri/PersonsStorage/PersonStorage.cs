using System.Linq;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using PersonsApplication.ReadModels;
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
        private readonly IEventStreamStorage<PersonEntity> eventingStorage;
        private readonly IQueryStorage<Person> queryStorage;

        public PersonStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            this.queryStorage = new GeneralQueryStorage<Person>(logger, domainFactory, repository);
            this.eventingStorage = new GeneralEventStreamStorage<PersonEntity>(logger, domainFactory, repository);
        }

        public PersonStorage(IEventStreamStorage<PersonEntity> eventingStorage, IQueryStorage<Person> queryStorage)
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

        public Person FindByEmailAddress(string emailAddress)
        {
            var persons = this.queryStorage.Query(Query.From<Person>()
                .Where(e => e.Email, ConditionOperator.EqualTo, emailAddress));
            return persons.Results.FirstOrDefault();
        }
    }
}