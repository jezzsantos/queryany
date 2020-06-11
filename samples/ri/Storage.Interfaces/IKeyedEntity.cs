using QueryAny;

namespace Storage.Interfaces
{
    public interface IKeyedEntity : INamedEntity
    {
        string Id { get; set; }
    }
}