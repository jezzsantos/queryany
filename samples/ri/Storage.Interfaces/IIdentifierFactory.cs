namespace Storage.Interfaces
{
    public interface IIdentifierFactory
    {
        string Create(IKeyedEntity entity);
    }
}