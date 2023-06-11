using System;
using Api.Common;
using Api.Interfaces.ServiceOperations.{{DomainName | string.pascalplural}};
using Application.Interfaces;
using Application.Interfaces.Resources;
using {{DomainName | string.pascalplural}}Application;
using Common;
using ServiceStack;

namespace {{DomainName | string.pascalplural}}ApiHost.Services.{{DomainName | string.pascalplural}}
{
    internal partial class {{DomainName | string.pascalplural}}Service : {{DomainName | string.pascalplural}}ServiceBase
    {
        public {{DomainName | string.pascalplural}}Service(I{{DomainName | string.pascalplural}}Application {{DomainName | string.camelplural}}Application)
            : base({{DomainName | string.camelplural}}Application)
        {
        }
    }

    internal abstract class {{DomainName | string.pascalplural}}ServiceBase : Service
    {
        private readonly I{{DomainName | string.pascalplural}}Application {{DomainName | string.camelplural}}Application;

        protected {{DomainName | string.pascalplural}}ServiceBase(I{{DomainName | string.pascalplural}}Application {{DomainName | string.camelplural}}Application)
        {
            {{DomainName | string.camelplural}}Application.GuardAgainstNull(nameof({{DomainName | string.camelplural}}Application));

            this.{{DomainName | string.camelplural}}Application = {{DomainName | string.camelplural}}Application;
        }
{{~for operation in ServiceOperation.Items~}}
{{if (operation.IsTestingOnly)}}#if TESTINGONLY{{end}}
{{~if (operation.Kind == "SEARCH")~}}
        public virtual {{operation.Name}}{{DomainName | string.pascalplural}}Response Get({{operation.Name}}{{DomainName | string.pascalplural}}Request request)
{{~else~}}
        public virtual {{operation.Name}}{{DomainName | string.pascalsingular}}Response {{kind = operation.Kind; case kind; when "POST"; "Post"; when "PUT"; "Any"; when "GET"; "Get"; when "SEARCH"; "Get"; when "DELETE"; "Delete"; end}}({{operation.Name}}{{DomainName | string.pascalsingular}}Request request)
{{~end~}}
        {
{{~if (operation.Kind == "SEARCH")~}}
            var {{DomainName | string.camelplural}} = this.{{DomainName | string.camelplural}}Application.{{operation.Name}}{{DomainName | string.pascalplural}}(Request.ToCaller()
                {{for field in operation.Request.Field.Items}}, request.{{field.Name}}{{end}},
                request.ToSearchOptions(defaultSort: nameof(Application.Interfaces.Resources.{{DomainName | string.pascalsingular}}.Id)),
                request.ToGetOptions());

            return new {{operation.Name}}{{DomainName | string.pascalplural}}Response
            {
                {{DomainName | string.pascalplural}} = {{DomainName | string.camelplural}}.Results,
                Metadata = {{DomainName | string.camelplural}}.Metadata
            };
{{~end~}}
{{~if (operation.Kind == "POST")~}}
            var {{DomainName | string.camelsingular}} = this.{{DomainName | string.camelplural}}Application.{{operation.Name}}{{DomainName | string.pascalsingular}}(Request.ToCaller()
                {{for field in operation.Request.Field.Items}}, request.{{field.Name}}{{end}});
            Response.SetLocation({{DomainName | string.camelsingular}});

            return new {{operation.Name}}{{DomainName | string.pascalsingular}}Response
            {
                {{DomainName | string.pascalsingular}} = {{DomainName | string.camelsingular}}
            };
{{~end~}}
{{~if (operation.Kind == "PUT" || operation.Kind == "GET")~}}
            var {{DomainName | string.camelsingular}} = this.{{DomainName | string.camelplural}}Application.{{operation.Name}}{{DomainName | string.pascalsingular}}(Request.ToCaller()
                {{for field in operation.Request.Field.Items}}, request.{{field.Name}}{{end}});

            return new {{operation.Name}}{{DomainName | string.pascalsingular}}Response
            {
                {{DomainName | string.pascalsingular}} = {{DomainName | string.camelsingular}}
            };
{{~end~}}
{{~if (operation.Kind == "DELETE")~}}
            var {{DomainName | string.camelsingular}} = this.{{DomainName | string.camelplural}}Application.{{operation.Name}}{{DomainName | string.pascalsingular}}(Request.ToCaller()
                {{for field in operation.Request.Field.Items}}, request.{{field.Name}}{{end}});

            return new {{operation.Name}}{{DomainName | string.pascalsingular}}Response();
{{~end~}}
        }
{{if (operation.IsTestingOnly)}}#endif{{end}}
{{~end~}}
    }
}