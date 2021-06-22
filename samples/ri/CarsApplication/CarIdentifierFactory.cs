using System;
using System.Collections.Generic;
using Application.Storage.Interfaces.ReadModels;
using CarsDomain;
using Domain.Interfaces.Entities;

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