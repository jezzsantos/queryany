namespace Storage.Interfaces
{
    public interface IIdentifiableEntity
    {
        string Id { get; }

        void Identify(string id);
    }
}