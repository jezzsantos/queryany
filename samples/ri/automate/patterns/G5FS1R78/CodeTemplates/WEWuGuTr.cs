using System;
using System.Collections.Generic;
using Application.Interfaces;
using {{DomainName | string.pascalplural}}Application.ReadModels;
using {{DomainName | string.pascalplural}}Domain;
using Domain.Interfaces.Entities;

namespace {{DomainName | string.pascalplural}}Application.Storage
{
    public interface I{{DomainName | string.pascalsingular}}Storage
    {
        {{DomainName | string.pascalsingular}}Entity Load(Identifier id);

        {{DomainName | string.pascalsingular}}Entity Save({{DomainName | string.pascalsingular}}Entity {{DomainName | string.camelsingular}});
    }
}