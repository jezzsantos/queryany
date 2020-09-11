using QueryAny;

namespace Storage.Interfaces
{
    public interface IReadModelEntity : IQueryableEntity
    {
        string Id { get; set; }
    }
}