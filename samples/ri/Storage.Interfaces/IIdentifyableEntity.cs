namespace Storage.Interfaces
{
    public interface IIdentifyableEntity
    {
        string Id { get; }

        void Identify(string id);
    }
}