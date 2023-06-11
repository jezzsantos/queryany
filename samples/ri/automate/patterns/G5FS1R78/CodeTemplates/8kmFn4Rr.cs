using System;
using ServiceStack;

namespace Api.Interfaces.ServiceOperations.{{Parent.DomainName | string.pascalplural}}
{
    [Route("{{Route}}", "{{ kind = Kind; case kind; when "POST"; "POST"; when "PUT"; "PUT;PATCH"; when "GET"; "GET"; when "SEARCH"; "GET"; when "DELETE"; "DELETE"; end}}")]
{{~if Kind == "SEARCH"~}}
    public class {{Name}}{{Parent.DomainName | string.pascalplural}}Request : SearchOperation<{{Name}}{{Parent.DomainName | string.pascalplural}}Response>
{{~else~}}
    public class {{Name}}{{Parent.DomainName | string.pascalsingular}}Request : {{ kind = Kind; case kind; when "POST"; "Post"; when "PUT"; "PutPatch"; when "GET"; "Get"; when "SEARCH"; "Search"; when "DELETE"; "Delete"; end}}Operation<{{Name}}{{Parent.DomainName | string.pascalsingular}}Response>
{{~end~}}
    {
{{~for field in Request.Field.Items~}}
        public {{field.DataType}}{{if (field.IsOptional && field.DataType != "string")}}?{{end}} {{field.Name}} { get; set; }

{{~end~}}
    }
}