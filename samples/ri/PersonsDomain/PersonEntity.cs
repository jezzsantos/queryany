using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace PersonsDomain
{
    public class PersonEntity : EntityBase
    {
        public PersonEntity(ILogger logger, IIdentifierFactory idFactory, PersonName name) : base(logger, idFactory)
        {
            name.GuardAgainstNull(nameof(name));
            Name = name;
        }

        public PersonName Name { get; private set; }

        public Email Email { get; private set; }

        public PhoneNumber Phone { get; private set; }

        public void SetEmail(Email email)
        {
            email.GuardAgainstNull(nameof(email));
            Email = email;
        }

        public void SetPhoneNumber(PhoneNumber number)
        {
            number.GuardAgainstNull(nameof(number));
            Phone = number;
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

        public static EntityFactory<PersonEntity> Rehydrate()
        {
            return (properties, container) =>
                new PersonEntity(container.Resolve<ILogger>(), new HydrationIdentifierFactory(properties),
                    properties.GetValueOrDefault<PersonName>(nameof(Name)));
        }
    }
}