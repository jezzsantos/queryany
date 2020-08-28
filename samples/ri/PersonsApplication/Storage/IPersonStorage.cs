using Domain.Interfaces.Entities;
using PersonsDomain;

namespace PersonsApplication.Storage
{
    public interface IPersonStorage
    {
        PersonEntity Load(Identifier id);

        PersonEntity Save(PersonEntity car);

        PersonEntity FindByEmailAddress(string emailAddress);
    }
}