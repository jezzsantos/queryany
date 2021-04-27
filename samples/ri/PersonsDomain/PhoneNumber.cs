using Domain.Interfaces;
using Domain.Interfaces.Entities;

namespace PersonsDomain
{
    public class PhoneNumber : SingleValueObjectBase<PhoneNumber, string>
    {
        public PhoneNumber(string phoneNumber) : base(phoneNumber)
        {
            phoneNumber.GuardAgainstNullOrEmpty(nameof(phoneNumber));
            phoneNumber.GuardAgainstInvalid(Validations.Person.PhoneNumber, nameof(phoneNumber));
        }

        public static ValueObjectFactory<PhoneNumber> Rehydrate()
        {
            return (property, container) => new PhoneNumber(property);
        }
    }
}