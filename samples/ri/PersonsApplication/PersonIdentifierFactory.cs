using System;
using System.Collections.Generic;
using Application.Storage.Interfaces.ReadModels;
using Domain.Interfaces.Entities;
using PersonsDomain;

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