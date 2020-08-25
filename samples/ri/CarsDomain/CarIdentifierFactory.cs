using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;

namespace CarsDomain
{
    public class CarIdentifierFactory : EntityPrefixIdentifierFactory
    {
        public CarIdentifierFactory() : base(new Dictionary<Type, string>
        {
            {typeof(CarEntity), "car"},
            {typeof(UnavailabilityEntity), "una"}
        })
        {
        }
    }
}