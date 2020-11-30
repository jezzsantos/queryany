using Domain.Interfaces.Entities;
using PersonsApplication.ReadModels;
using PersonsDomain;

namespace PersonsApplication.Storage
{
    public interface IPersonStorage
    {
        PersonEntity Load(Identifier id);

        PersonEntity Save(PersonEntity person);

        Person FindByEmailAddress(string emailAddress);

        Person GetPerson(Identifier id);
    }
}