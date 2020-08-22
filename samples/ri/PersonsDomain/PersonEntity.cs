using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace PersonsDomain
{
    public class PersonEntity : EntityBase
    {
        public PersonEntity(ILogger logger, IIdentifierFactory idFactory, PersonName name) : base(logger, idFactory)
        {
            RaiseCreateEvent(PersonsDomain.Events.Person.Created.Create(Id, name));
        }

        public PersonName Name { get; private set; }

        public Email Email { get; private set; }

        public PhoneNumber Phone { get; private set; }

        public void SetEmail(Email email)
        {
            RaiseChangeEvent(PersonsDomain.Events.Person.EmailChanged.Create(Id, email));
        }

        public void SetPhoneNumber(PhoneNumber number)
        {
            RaiseChangeEvent(PersonsDomain.Events.Person.PhoneNumberChanged.Create(Id, number));
        }

        protected override void When(object @event)
        {
            switch (@event)
            {
                case Events.Person.Created created:
                    Name = new PersonName(created.FirstName, created.LastName);
                    break;

                case Events.Person.EmailChanged changed:
                    Email = new Email(changed.EmailAddress);
                    break;

                case Events.Person.PhoneNumberChanged changed:
                    Phone = new PhoneNumber(changed.Number);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown event {@event.GetType()}");
            }
        }

        public override Dictionary<string, object> Dehydrate()
        {
            var properties = base.Dehydrate();
            properties.Add(nameof(Name), Name);
            properties.Add(nameof(Email), Email);
            properties.Add(nameof(Phone), Phone);
            return properties;
        }

        public override void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            base.Rehydrate(properties);
            Name = properties.GetValueOrDefault<PersonName>(nameof(Name));
            Email = properties.GetValueOrDefault<Email>(nameof(Email));
            Phone = properties.GetValueOrDefault<PhoneNumber>(nameof(Phone));
        }

        public static EntityFactory<PersonEntity> Instantiate()
        {
            return (properties, container) =>
                new PersonEntity(container.Resolve<ILogger>(), new HydrationIdentifierFactory(properties),
                    properties.GetValueOrDefault<PersonName>(nameof(Name)));
        }
    }
}