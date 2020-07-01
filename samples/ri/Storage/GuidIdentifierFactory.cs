using System;
using Storage.Interfaces;

namespace Storage
{
    public class GuidIdentifierFactory : IIdentifierFactory
    {
        public string Create(IIdentifyableEntity entity)
        {
            return Guid.NewGuid().ToString("D");
        }
    }
}