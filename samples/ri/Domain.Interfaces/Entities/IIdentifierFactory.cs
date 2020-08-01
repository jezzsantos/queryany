namespace Services.Interfaces.Entities
{
    public interface IIdentifierFactory
    {
        Identifier Create(IIdentifiableEntity entity);

        bool IsValid(Identifier value);
    }
}