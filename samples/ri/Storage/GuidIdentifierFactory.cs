using System;
using Domain.Interfaces.Entities;

namespace Storage
{
    public class GuidIdentifierFactory : IIdentifierFactory
    {
        public Identifier Create(IIdentifiableEntity entity)
        {
            return Identifier.Create(Guid.NewGuid().ToString("D"));
        }

        public bool IsValid(Identifier value)
        {
            if (!value.HasValue())
            {
                return false;
            }

            if (!Guid.TryParse(value.Get(), out var result))
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