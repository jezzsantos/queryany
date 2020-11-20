namespace Domain.Interfaces.Entities
{
    public interface IChangeEventMigrator
    {
        IChangeEvent Rehydrate(string eventId, string eventJson, string originalEventTypeName);
    }
}