using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;

namespace ServiceClients
{
    public interface IPersonsService
    {
        Person Get(Identifier id);
    }
}