using System.Collections.Generic;
using QueryAny.Primitives;

namespace Services.Interfaces.Entities
{
    public class Identifier : ValueType<Identifier>
    {
        private string identifier;

        private Identifier(string identifier)
        {
            Guard.AgainstNullOrEmpty(() => identifier, identifier);
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

        public string Get()
        {
            return Dehydrate();
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
}