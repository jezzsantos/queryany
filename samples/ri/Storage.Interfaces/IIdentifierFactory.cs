using Services.Interfaces.Entities;

namespace Storage.Interfaces
{
    public interface IIdentifierFactory
    {
        Identifier Create(IIdentifiableEntity entity);

        bool IsValid(Identifier value);
    }
}