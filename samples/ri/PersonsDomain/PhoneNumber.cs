using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace PersonsDomain
{
    public class PhoneNumber : ValueObjectBase<PhoneNumber>
    {
        private string phoneNumber;

        public PhoneNumber(string phoneNumber)
        {
            phoneNumber.GuardAgainstNullOrEmpty(nameof(phoneNumber));
            phoneNumber.GuardAgainstInvalid(Validations.Person.PhoneNumber, nameof(phoneNumber));

            this.phoneNumber = phoneNumber;
        }

        public override void Rehydrate(string value)
        {
            this.phoneNumber = value;
        }

        public static ValueObjectFactory<PhoneNumber> Rehydrate()
        {
            return (property, container) => new PhoneNumber(property);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {this.phoneNumber};
        }
    }
}