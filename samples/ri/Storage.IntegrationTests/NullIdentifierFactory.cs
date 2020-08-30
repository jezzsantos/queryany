using System;
using Domain.Interfaces.Entities;

namespace Storage.IntegrationTests
{
    public class NullIdentifierFactory : IIdentifierFactory
    {
        public Identifier Create(IIdentifiableEntity entity)
        {
            return null;
        }

        public bool IsValid(Identifier value)
        {
            throw new NotImplementedException();
        }
    }
}