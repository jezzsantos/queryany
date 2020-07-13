namespace Services.Interfaces.Entities
{
    public interface IPersistableValueType
    {
        string Dehydrate();

        void Rehydrate(string value);
    }
}