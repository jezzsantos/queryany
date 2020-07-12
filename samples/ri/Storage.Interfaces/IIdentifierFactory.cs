namespace Storage.Interfaces
{
    public interface IIdentifierFactory
    {
        string Create(IIdentifiableEntity entity);
    }
}