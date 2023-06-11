using System.Collections.Generic;

namespace Application.Interfaces.Resources
{
    public class {{DomainName | string.pascalsingular}} : IIdentifiableResource
    {
        public string Id { get; set; }
    }
{{~for operation in ServiceOperation.Items~}}
{{if (operation.IsTestingOnly)}}#if TESTINGONLY{{end}}
{{~if (operation.Kind == "SEARCH")~}}
    public class {{operation.Name}}{{DomainName | string.pascalsingular}} : IIdentifiableResource
    {
        public string Id { get; set; }
    }
{{~end~}}
{{if (operation.IsTestingOnly)}}#endif{{end}}
{{~end~}}
}