using QueryAny;
using Storage.Interfaces;
using Storage.Interfaces.ReadModels;

namespace PersonsApplication.ReadModels
{
    [EntityName("Person")]
    public class Person : IReadModelEntity, IHasIdentity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Id { get; set; }
    }
}