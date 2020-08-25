using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;

namespace PersonsDomain
{
    public class PersonIdentifierFactory : EntityPrefixIdentifierFactory
    {
        public PersonIdentifierFactory() : base(new Dictionary<Type, string>
        {
            {typeof(PersonEntity), "per"}
        })
        {
        }
    }
}