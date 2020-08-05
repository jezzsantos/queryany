namespace Domain.Interfaces.Entities
{
    public interface IIdentifiableEntity
    {
        Identifier Id { get; }

        void Identify(Identifier id);
    }
}