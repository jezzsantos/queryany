using Domain.Interfaces.Resources;

namespace ApplicationServices
{
    public interface IPersonsService
    {
        Person Get(string id);
    }
}