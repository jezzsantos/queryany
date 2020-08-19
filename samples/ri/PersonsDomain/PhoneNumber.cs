using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace PersonsDomain
{
    public class PhoneNumber : SingleValueObjectBase<PhoneNumber, string>
    {
        public PhoneNumber(string phoneNumber) : base(phoneNumber)
        {
            phoneNumber.GuardAgainstNullOrEmpty(nameof(phoneNumber));
            phoneNumber.GuardAgainstInvalid(Validations.Person.PhoneNumber, nameof(phoneNumber));
        }

        protected override string ToValue(string value)
        {
            return value;
        }

        public static ValueObjectFactory<PhoneNumber> Instantiate()
        {
            return (property, container) => new PhoneNumber(property);
        }
    }
}