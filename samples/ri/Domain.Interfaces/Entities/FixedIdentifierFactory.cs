namespace Domain.Interfaces.Entities
{
    public class FixedIdentifierFactory : IIdentifierFactory
    {
        private readonly Identifier id;

        public FixedIdentifierFactory(string identifier)
        {
            identifier.GuardAgainstNull(nameof(identifier));
            this.id = identifier.ToIdentifier();
        }

        public Identifier Create(IIdentifiableEntity entity)
        {
            return this.id;
        }

        public bool IsValid(Identifier value)
        {
            return this.id == value;
        }
    }
}