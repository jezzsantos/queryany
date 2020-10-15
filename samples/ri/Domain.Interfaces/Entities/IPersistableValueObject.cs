namespace Domain.Interfaces.Entities
{
    public interface IPersistableValueObject
    {
        string Dehydrate();

        void Rehydrate(string value);
    }
}