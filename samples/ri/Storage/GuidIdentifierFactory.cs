using System;
using Services.Interfaces.Entities;
using Storage.Interfaces;

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