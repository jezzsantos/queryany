using System.Collections.Generic;
using Common;
using Domain.Interfaces;
using Domain.Interfaces.Entities;

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

        public string FirstName { get; }

        public string LastName { get; }

        public static ValueObjectFactory<PersonName> Rehydrate()
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