namespace Domain.Interfaces.Entities
{
    public class EventMetadata : SingleValueObjectBase<EventMetadata, string>
    {
        public EventMetadata(string fqn) : base(fqn)
        {
        }

        public string Fqn => Value;

        protected override string ToValue(string value)
        {
            return value;
        }

        public static ValueObjectFactory<EventMetadata> Instantiate()
        {
            return (property, container) => new EventMetadata(property);
        }
    }
}