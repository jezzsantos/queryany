using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace PersonsDomain
{
    public class Email : ValueObjectBase<Email>
    {
        private string emailAddress;

        public Email(string emailAddress)
        {
            emailAddress.GuardAgainstNullOrEmpty(nameof(emailAddress));
            emailAddress.GuardAgainstInvalid(Validations.Person.Email, nameof(emailAddress));

            this.emailAddress = emailAddress;
        }

        public override void Rehydrate(string value)
        {
            this.emailAddress = value;
        }

        public static ValueObjectFactory<Email> Rehydrate()
        {
            return (property, container) => new Email(property);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {this.emailAddress};
        }
    }
}