using System;
using Domain.Interfaces.Entities;

namespace Storage.IntegrationTests
{
    public class GuidIdentifierFactory : IIdentifierFactory
    {
        public Identifier Create(IIdentifiableEntity entity)
        {
            return Guid.NewGuid().ToString("D").ToIdentifier();
        }

        public bool IsValid(Identifier value)
        {
            if (!value.HasValue())
            {
                return false;
            }

            if (!Guid.TryParse(value, out var result))
            {
                return false;
            }

            if (result == Guid.Empty)
            {
                return false;
            }

            return true;
        }
    }
}