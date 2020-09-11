using Domain.Interfaces.Entities;
using PersonsApplication.ReadModels;
using PersonsDomain;

namespace PersonsApplication.Storage
{
    public interface IPersonStorage
    {
        PersonEntity Load(Identifier id);

        PersonEntity Save(PersonEntity car);

        Person FindByEmailAddress(string emailAddress);
    }
}