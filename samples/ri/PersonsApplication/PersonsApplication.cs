using Application;
using Application.Resources;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices;
using Microsoft.Extensions.Logging;
using PersonsApplication.Storage;
using PersonsDomain;
using QueryAny.Primitives;
using ServiceStack;
using PersonName = PersonsDomain.PersonName;

namespace PersonsApplication
{
    public class PersonsApplication : ApplicationBase, IPersonsApplication
    {
        private readonly IEmailService emailService;
        private readonly IIdentifierFactory idFactory;
        private readonly ILogger logger;
        private readonly IPersonStorage storage;

        public PersonsApplication(ILogger logger, IIdentifierFactory idFactory, IPersonStorage storage,
            IEmailService emailService)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            storage.GuardAgainstNull(nameof(storage));
            emailService.GuardAgainstNull(nameof(emailService));
            this.logger = logger;
            this.idFactory = idFactory;
            this.storage = storage;
            this.emailService = emailService;
        }

        public Person Create(ICurrentCaller caller, string firstName, string lastName)
        {
            caller.GuardAgainstNull(nameof(caller));

            var person = new PersonEntity(this.logger, this.idFactory, this.emailService);
            person.SetName(new PersonName(firstName, lastName));

            var created = this.storage.Save(person);

            this.logger.LogInformation("Person {Id} was created by {Caller}", created.Id, caller.Id);

            return created.ToPerson();
        }

        public Person Get(ICurrentCaller caller, string id, GetOptions options)
        {
            caller.GuardAgainstNull(nameof(caller));
            id.GuardAgainstNullOrEmpty(nameof(id));

            if (id.ToIdentifier() == CurrentCallerConstants.AnonymousUserId)
            {
                return Person.Anonymous;
            }

            var person = this.storage.Load(id.ToIdentifier());
            if (id == null)
            {
                throw new ResourceNotFoundException();
            }

            return person.ToPerson();
        }
    }

    public static class PersonConversionExtensions
    {
        public static Person ToPerson(this PersonEntity entity)
        {
            var dto = entity.ConvertTo<Person>();
            dto.Id = entity.Id;
            dto.DisplayName = entity.DisplayName?.DisplayName;
            return dto;
        }
    }
}