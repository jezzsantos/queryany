using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace PersonsDomain
{
    public class PersonName : ValueObjectBase<PersonName>
    {
        public PersonName(string firstName, string lastName)
        {
            firstName.GuardAgainstNullOrEmpty(nameof(firstName));
            lastName.GuardAgainstNullOrEmpty(nameof(lastName));
            firstName.GuardAgainstInvalid(Validations.Person.Name, nameof(firstName));
            lastName.GuardAgainstInvalid(Validations.Person.Name, nameof(lastName));

            FirstName = firstName;
            LastName = lastName;
        }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public override void Rehydrate(string value)
        {
            var parts = RehydrateToList(value);
            FirstName = parts[0];
            LastName = parts[1];
        }

        public static ValueObjectFactory<PersonName> Instantiate()
        {
            return (property, container) =>
            {
                var parts = RehydrateToList(property, false);
                return new PersonName(parts[0], parts[1]);
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {FirstName, LastName};
        }
    }
}