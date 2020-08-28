using System;
using Domain.Interfaces.Entities;

namespace Storage.UnitTests
{
    public class FixedIdentifierFactory : IIdentifierFactory
    {
        private readonly string identifier;

        public FixedIdentifierFactory(string identifier)
        {
            this.identifier = identifier;
        }

        public Identifier Create(IIdentifiableEntity entity)
        {
            return this.identifier.ToIdentifier();
        }

        public bool IsValid(Identifier value)
        {
            throw new NotImplementedException();
        }
    }
}