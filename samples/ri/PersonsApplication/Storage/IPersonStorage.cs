using Domain.Interfaces.Entities;
using PersonsDomain;

namespace PersonsApplication.Storage
{
    public interface IPersonStorage
    {
        PersonEntity Get(Identifier toIdentifier);

        PersonEntity Create(PersonEntity person);

        PersonEntity FindByEmailAddress(string emailAddress);
    }
}