using DomainServices;
using PersonsApplication.Storage;
using QueryAny.Primitives;

namespace PersonsApplication
{
    public class EmailService : IEmailService
    {
        private readonly IPersonStorage storage;

        public EmailService(IPersonStorage storage)
        {
            storage.GuardAgainstNull(nameof(storage));
            this.storage = storage;
        }

        public bool EnsureEmailIsUnique(string emailAddress, string personId)
        {
            emailAddress.GuardAgainstNullOrEmpty(nameof(emailAddress));
            personId.GuardAgainstNullOrEmpty(nameof(personId));

            var person = this.storage.FindByEmailAddress(emailAddress);
            if (person == null)
            {
                return true;
            }

            return person.Id.Equals(personId);
        }
    }
}