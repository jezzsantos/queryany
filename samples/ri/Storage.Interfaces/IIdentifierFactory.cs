namespace Storage.Interfaces
{
    public interface IIdentifierFactory
    {
        string Create(IIdentifyableEntity entity);
    }
}