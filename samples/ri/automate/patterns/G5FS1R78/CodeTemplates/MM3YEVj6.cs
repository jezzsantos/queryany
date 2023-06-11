using System;
using System.Collections.Generic;
using Application.Storage.Interfaces.ReadModels;
using {{DomainName | string.pascalplural}}Domain;
using Domain.Interfaces.Entities;

namespace {{DomainName | string.pascalplural}}Application
{
    public class {{DomainName | string.pascalsingular}}IdentifierFactory : EntityPrefixIdentifierFactory
    {
        public {{DomainName | string.pascalsingular}}IdentifierFactory() : base(new Dictionary<Type, string>
        {
            {typeof(Checkpoint), "ckp"},
            {typeof({{DomainName | string.pascalsingular}}Entity), "{{DomainName | string.camelsingular}}"},
        })
        {
        }
    }
}