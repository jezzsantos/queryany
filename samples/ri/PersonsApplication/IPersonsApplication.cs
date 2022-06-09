using Application.Interfaces;
using Application.Interfaces.Resources;

namespace PersonsApplication
{
    public interface IPersonsApplication
    {
        Person Get(ICurrentCaller caller, string id, GetOptions options);

        Person Create(ICurrentCaller caller, string firstName, string lastName);
    }
}