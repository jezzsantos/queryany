using Application;
using Application.Interfaces;
using Application.Resources;
using Common;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices.Interfaces;
using PersonsApplication.Storage;
using PersonsDomain;
using ServiceStack;
using PersonName = PersonsDomain.PersonName;

namespace PersonsApplication
{
    public class PersonsApplication : ApplicationBase, IPersonsApplication
    {
        private readonly IEmailService emailService;
        private readonly IIdentifierFactory idFactory;
        private readonly IRecorder recorder;
        private readonly IPersonStorage storage;

        public PersonsApplication(IRecorder recorder, IIdentifierFactory idFactory, IPersonStorage storage,
            IEmailService emailService)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            idFactory.GuardAgainstNull(nameof(idFactory));
            storage.GuardAgainstNull(nameof(storage));
            emailService.GuardAgainstNull(nameof(emailService));
            this.recorder = recorder;
            this.idFactory = idFactory;
            this.storage = storage;
            this.emailService = emailService;
        }

        public Person Create(ICurrentCaller caller, string firstName, string lastName)
        {
            caller.GuardAgainstNull(nameof(caller));

            var person = new PersonEntity(this.recorder, this.idFactory, this.emailService);
            person.SetName(new PersonName(firstName, lastName));

            var created = this.storage.Save(person);

            this.recorder.TraceInformation("Person {Id} was created by {Caller}", created.Id, caller.Id);

            return created.ToPerson();
        }

        public Person Get(ICurrentCaller caller, string id, GetOptions options)
        {
            caller.GuardAgainstNull(nameof(caller));
            id.GuardAgainstNullOrEmpty(nameof(id));

            if (id.ToIdentifier() == CallerConstants.AnonymousUserId)
            {
                return Person.Anonymous;
            }

            var person = this.storage.GetPerson(id.ToIdentifier());
            if (person == null)
            {
                throw new ResourceNotFoundException();
            }

            return person.ToPerson();
        }
    }

    public static class PersonConversionExtensions
    {
        public static Person ToPerson(this ReadModels.Person readModel)
        {
            var dto = readModel.ConvertTo<Person>();
            dto.Name = new Application.Resources.PersonName
                {FirstName = readModel.FirstName, LastName = readModel.LastName};
            return dto;
        }

        public static Person ToPerson(this PersonEntity entity)
        {
            var dto = entity.ConvertTo<Person>();
            dto.Id = entity.Id;
            dto.DisplayName = entity.DisplayName?.DisplayName;
            return dto;
        }
    }
}