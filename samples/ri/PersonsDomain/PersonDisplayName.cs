using Domain.Interfaces.Entities;

namespace PersonsDomain
{
    public class PersonDisplayName : SingleValueObjectBase<PersonDisplayName, string>
    {
        public PersonDisplayName(string displayName) : base(displayName)
        {
        }

        public string DisplayName => Value;

        public static ValueObjectFactory<PersonDisplayName> Rehydrate()
        {
            return (property, container) => new PersonDisplayName(property);
        }
    }
}