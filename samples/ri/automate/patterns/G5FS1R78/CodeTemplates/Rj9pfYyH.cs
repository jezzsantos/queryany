using System;
using Application.Interfaces;
using Application.Interfaces.Resources;

namespace {{DomainName | string.pascalplural}}Application
{
    public partial interface I{{DomainName | string.pascalplural}}Application
    {
{{~for operation in ServiceOperation.Items~}}
    {{~if (operation.IsTestingOnly)~}}
#if TESTINGONLY
    {{~end~}}
    {{~if (operation.Kind == "SEARCH")~}}
        SearchResults<Application.Interfaces.Resources.{{operation.Name}}{{DomainName | string.pascalsingular}}> {{operation.Name}}{{DomainName | string.pascalplural}}(ICurrentCaller caller{{for field in operation.Request.Field.Items}}, {{field.DataType}}{{if (field.IsOptional && field.DataType != "string")}}?{{end}} {{field.Name | string.camelcase}}{{end}}, SearchOptions searchOptions, GetOptions getOptions);
    {{~else~}}
        Application.Interfaces.Resources.{{DomainName | string.pascalsingular}} {{operation.Name}}{{DomainName | string.pascalsingular}}(ICurrentCaller caller{{for field in operation.Request.Field.Items}}, {{field.DataType}}{{if (field.IsOptional && field.DataType != "string")}}?{{end}} {{field.Name | string.camelcase}}{{end}});
    {{~end~}}
    {{~if (operation.IsTestingOnly)~}}
#endif
    {{~end~}}

{{~end~}}
    }
}