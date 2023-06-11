{{~if Kind == "SEARCH"~}}
using System.Collections.Generic;
{{~end~}}
using ServiceStack;

namespace Api.Interfaces.ServiceOperations.{{Parent.DomainName | string.pascalplural}}
{
{{~if Kind == "SEARCH"~}}
    public class {{Name}}{{Parent.DomainName | string.pascalplural}}Response : SearchOperationResponse
{{~else~}}
    public class {{Name}}{{Parent.DomainName | string.pascalsingular}}Response
{{~end~}}
    {
        public ResponseStatus ResponseStatus { get; set; }
{{~if Kind == "SEARCH"~}}

        public List<Application.Interfaces.Resources.{{Name}}{{Parent.DomainName | string.pascalsingular}}> {{Parent.DomainName | string.pascalplural}} { get; set; }
{{~else if (Kind != "DELETE")~}}

        public Application.Interfaces.Resources.{{Parent.DomainName | string.pascalsingular}} {{Parent.DomainName | string.pascalsingular}} { get; set; }
{{~end~}}
    }
}