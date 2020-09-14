using QueryAny;

namespace Storage.Interfaces.ReadModels
{
    public interface IReadModelEntity : IQueryableEntity
    {
        string Id { get; set; }
    }
}