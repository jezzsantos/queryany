using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using PersonsDomain;
using Storage.Interfaces.ReadModels;

namespace PersonsApplication
{
    public class PersonIdentifierFactory : EntityPrefixIdentifierFactory
    {
        public PersonIdentifierFactory() : base(new Dictionary<Type, string>
        {
            {typeof(Checkpoint), "ckp"},
            {typeof(PersonEntity), "per"}
        })
        {
        }
    }
}