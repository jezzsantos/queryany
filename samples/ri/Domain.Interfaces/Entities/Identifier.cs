using System.Collections.Generic;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    public class Identifier : ValueObjectBase<Identifier>
    {
        private string identifier;

        private Identifier(string identifier)
        {
            identifier.GuardAgainstNullOrEmpty(nameof(identifier));
            this.identifier = identifier;
        }

        public override string Dehydrate()
        {
            return this.identifier;
        }

        public override void Rehydrate(string value)
        {
            this.identifier = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {this.identifier};
        }

        public static Identifier Create(string value)
        {
            return new Identifier(value);
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