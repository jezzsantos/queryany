using System.Collections.Generic;
using Application.Storage.Interfaces.ReadModels;
using QueryAny;

namespace {{DomainName | string.pascalplural}}Application.ReadModels
{
    [EntityName("{{DomainName | string.pascalsingular}}")]
    public class {{DomainName | string.pascalsingular}} : ReadModelEntity
    {
    }
}