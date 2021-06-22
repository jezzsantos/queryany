using Application.Resources;

namespace ApplicationServices.Interfaces
{
    public interface IPersonsService
    {
        Person Get(string id);
    }
}