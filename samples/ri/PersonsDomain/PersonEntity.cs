using System;
using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices;
using Microsoft.Extensions.Logging;
using PersonsDomain.Properties;
using QueryAny.Primitives;

namespace PersonsDomain
{
    public class PersonEntity : AggregateRootBase
    {
        private readonly IEmailService emailService;

        public PersonEntity(ILogger logger, IIdentifierFactory idFactory, IEmailService emailService,
            PersonName name) : base(logger, idFactory)
        {
            emailService.GuardAgainstNull(nameof(emailService));
            this.emailService = emailService;
            RaiseCreateEvent(PersonsDomain.Events.Person.Created.Create(Id, name));
        }

        public PersonName Name { get; private set; }

        public PersonDisplayName DisplayName { get; private set; }

        public Email Email { get; private set; }

        public PhoneNumber Phone { get; private set; }

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

        protected override void OnEventRaised(object @event)
        {
            switch (@event)
            {
                case Events.Person.Created created:
                    Name = new PersonName(created.FirstName, created.LastName);
                    DisplayName = new PersonDisplayName(created.FirstName);
                    Logger.LogDebug("Person {Id} created with name {FirstName}, {LastName}", Id, created.FirstName,
                        created.LastName);
                    break;

                case Events.Person.EmailChanged changed:
                    Email = new Email(changed.EmailAddress);
                    Logger.LogDebug("Person {Id} changed email to {EmailAddress}", Id, changed.EmailAddress);
                    break;

                case Events.Person.PhoneNumberChanged changed:
                    Phone = new PhoneNumber(changed.PhoneNumber);
                    Logger.LogDebug("Person {Id} changed phone number to {PhoneNumber}", Id, changed.PhoneNumber);
                    break;

                case Events.Person.DisplayNameChanged changed:
                    DisplayName = new PersonDisplayName(changed.DisplayName);
                    Logger.LogDebug("Person {Id} changed display name to {DisplayName}", Id, changed.DisplayName);
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

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(Name), Name);
            properties.Add(nameof(DisplayName), DisplayName);
            properties.Add(nameof(Email), Email);
            properties.Add(nameof(Phone), Phone);
            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            Name = properties.GetValueOrDefault<PersonName>(nameof(Name));
            DisplayName = properties.GetValueOrDefault<PersonDisplayName>(nameof(DisplayName));
            Email = properties.GetValueOrDefault<Email>(nameof(Email));
            Phone = properties.GetValueOrDefault<PhoneNumber>(nameof(Phone));
        }

        public static EntityFactory<PersonEntity> Instantiate()
        {
            return (properties, container) =>
                new PersonEntity(container.Resolve<ILogger>(), new HydrationIdentifierFactory(properties),
                    container.Resolve<IEmailService>(),
                    properties.GetValueOrDefault<PersonName>(nameof(Name)));
        }
    }
}