namespace Storage.Interfaces
{
    public interface IIdentifierFactory
    {
        string Create(IIdentifiableEntity entity);
        
        bool IsValid(string value);
    }
}