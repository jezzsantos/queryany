using System;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices;
using Microsoft.Extensions.Logging;
using PersonsDomain.Properties;
using QueryAny;
using QueryAny.Primitives;

namespace PersonsDomain
{
    [EntityName("Person")]
    public class PersonEntity : AggregateRootBase
    {
        private readonly IEmailService emailService;

        public PersonEntity(ILogger logger, IIdentifierFactory idFactory, IEmailService emailService) : base(logger,
            idFactory, PersonsDomain.Events.Person.Created.Create)
        {
            emailService.GuardAgainstNull(nameof(emailService));
            this.emailService = emailService;
        }

        private PersonEntity(ILogger logger, IIdentifierFactory idFactory, IEmailService emailService,
            Identifier identifier) : base(logger,
            idFactory, identifier)
        {
            emailService.GuardAgainstNull(nameof(emailService));
            this.emailService = emailService;
        }

        public PersonName Name { get; private set; }

        public PersonDisplayName DisplayName { get; private set; }

        public Email Email { get; private set; }

        public PhoneNumber Phone { get; private set; }

        public void SetName(PersonName name)
        {
            RaiseChangeEvent(PersonsDomain.Events.Person.NameChanged.Create(Id, name));
        }

        public void SetDisplayName(PersonDisplayName name)
        {
            RaiseChangeEvent(PersonsDomain.Events.Person.DisplayNameChanged.Create(Id, name));
        }

        public void SetEmail(Email email)
        {
            RaiseChangeEvent(PersonsDomain.Events.Person.EmailChanged.Create(Id, email));
        }

        public void SetPhoneNumber(PhoneNumber number)
        {
            RaiseChangeEvent(PersonsDomain.Events.Person.PhoneNumberChanged.Create(Id, number));
        }

        protected override void OnStateChanged(IChangeEvent @event)
        {
            switch (@event)
            {
                case Events.Person.Created _:
                    break;

                case Events.Person.NameChanged changed:
                    Name = new PersonName(changed.FirstName, changed.LastName);
                    DisplayName = new PersonDisplayName(changed.FirstName);
                    Logger.LogDebug("Person {Id} changed name to {FirstName}, {LastName}", Id, changed.FirstName,
                        changed.LastName);
                    break;

                case Events.Person.DisplayNameChanged changed:
                    DisplayName = new PersonDisplayName(changed.DisplayName);
                    Logger.LogDebug("Person {Id} changed display name to {DisplayName}", Id, changed.DisplayName);
                    break;

                case Events.Person.EmailChanged changed:
                    Email = new Email(changed.EmailAddress);
                    Logger.LogDebug("Person {Id} changed email to {EmailAddress}", Id, changed.EmailAddress);
                    break;

                case Events.Person.PhoneNumberChanged changed:
                    Phone = new PhoneNumber(changed.PhoneNumber);
                    Logger.LogDebug("Person {Id} changed phone number to {PhoneNumber}", Id, changed.PhoneNumber);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown event {@event.GetType()}");
            }
        }

        protected override bool EnsureValidState()
        {
            var isValid = base.EnsureValidState();

            if (Email.HasValue())
            {
                if (!this.emailService.EnsureEmailIsUnique(Email, Id))
                {
                    throw new RuleViolationException(Resources.PersonEntity_EmailNotUnique);
                }
            }

            return isValid;
        }

        public static AggregateRootFactory<PersonEntity> Instantiate()
        {
            return (identifier, container, rehydratingProperties) =>
                new PersonEntity(container.Resolve<ILogger>(), container.Resolve<IIdentifierFactory>(),
                    container.Resolve<IEmailService>(), identifier);
        }
    }
}