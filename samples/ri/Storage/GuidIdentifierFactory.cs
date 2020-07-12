using System;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage
{
    public class GuidIdentifierFactory : IIdentifierFactory
    {
        public string Create(IIdentifiableEntity entity)
        {
            return Guid.NewGuid().ToString("D");
        }

        public bool IsValid(string value)
        {
            if (!value.HasValue())
            {
                return false;
            }

            return Guid.TryParse(value, out _);
        }
    }
}