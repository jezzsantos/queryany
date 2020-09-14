using System;
using System.Collections.Generic;
using CarsDomain;
using Domain.Interfaces.Entities;
using Storage.Interfaces.ReadModels;

namespace CarsApplication
{
    public class CarIdentifierFactory : EntityPrefixIdentifierFactory
    {
        public CarIdentifierFactory() : base(new Dictionary<Type, string>
        {
            {typeof(Checkpoint), "ckp"},
            {typeof(CarEntity), "car"},
            {typeof(UnavailabilityEntity), "una"}
        })
        {
        }
    }
}