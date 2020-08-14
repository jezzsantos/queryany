namespace Domain.Interfaces.Resources
{
    public class Person : IIdentifiableResource
    {
        public PersonName Name { get; set; }

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