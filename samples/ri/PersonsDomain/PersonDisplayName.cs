using Domain.Interfaces.Entities;

namespace PersonsDomain
{
    public class PersonDisplayName : SingleValueObjectBase<PersonDisplayName, string>
    {
        public PersonDisplayName(string displayName) : base(displayName)
        {
        }

        public string DisplayName => Value;

        protected override string ToValue(string value)
        {
            return value;
        }

        public static ValueObjectFactory<PersonDisplayName> Instantiate()
        {
            return (property, container) => new PersonDisplayName(property);
        }
    }
}