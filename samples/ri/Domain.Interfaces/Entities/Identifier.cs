using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    public class Identifier : SingleValueObjectBase<Identifier, string>
    {
        private Identifier(string identifier) : base(identifier)
        {
            identifier.GuardAgainstNullOrEmpty(nameof(identifier));
        }

        public static Identifier Create(string value)
        {
            return new Identifier(value);
        }

        protected override string ToValue(string value)
        {
            return value;
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