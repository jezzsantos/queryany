using Domain.Interfaces;

namespace Application.Interfaces.Resources
{
    public class Person : IIdentifiableResource
    {
        public static readonly Person Anonymous = new Person
        {
            Id = CallerConstants.AnonymousUserId,
            Name = new PersonName
            {
                FirstName = CallerConstants.AnonymousUserName,
                LastName = CallerConstants.AnonymousUserName
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