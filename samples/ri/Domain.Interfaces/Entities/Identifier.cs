using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    public class Identifier : SingleValueObjectBase<Identifier, string>
    {
        private Identifier(string identifier) : base(identifier)
        {
            identifier.GuardAgainstNullOrEmpty(nameof(identifier));
        }

        private Identifier() : base(string.Empty)
        {
        }

        public static Identifier Empty()
        {
            return new Identifier();
        }

        public bool IsEmpty()
        {
            return !Value.HasValue();
        }

        public static Identifier Create(string value)
        {
            return new Identifier(value);
        }

        protected override string ToValue(string value)
        {
            return value;
        }

        public static ValueObjectFactory<Identifier> Instantiate()
        {
            return (property, container) => new Identifier(property);
        }
    }

    public static class IdentifierExtensions
    {
        public static Identifier ToIdentifier(this string id)
        {
            return Identifier.Create(id);
        }
    }
}