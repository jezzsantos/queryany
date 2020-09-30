using Domain.Interfaces;

namespace Application.Resources
{
    public class Person : IIdentifiableResource
    {
        public static readonly Person Anonymous = new Person
        {
            Id = CurrentCallerConstants.AnonymousUserId,
            Name = new PersonName
            {
                FirstName = CurrentCallerConstants.AnonymousUserName,
                LastName = CurrentCallerConstants.AnonymousUserName
            },
            Email = null,
            PhoneNumber = null
        };

        public PersonName Name { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Id { get; set; }
    }

    public class PersonName
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}