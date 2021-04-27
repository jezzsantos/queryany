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

        public static ValueObjectFactory<Email> Rehydrate()
        {
            return (property, container) => new Email(property);
        }
    }
}