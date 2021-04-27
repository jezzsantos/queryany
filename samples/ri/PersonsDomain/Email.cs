using Domain.Interfaces;
using Domain.Interfaces.Entities;

namespace PersonsDomain
{
    public class Email : SingleValueObjectBase<Email, string>
    {
        public Email(string emailAddress) : base(emailAddress)
        {
            emailAddress.GuardAgainstNullOrEmpty(nameof(emailAddress));
            emailAddress.GuardAgainstInvalid(Domain.Interfaces.Validations.Email, nameof(emailAddress));
        }

        protected override string ToValue(string value)
        {
            return value;
        }

        public static ValueObjectFactory<Email> Instantiate()
        {
            return (property, container) => new Email(property);
        }
    }
}