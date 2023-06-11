using {{Parent.DomainName | string.pascalplural}}Api.Properties;
using {{Parent.DomainName | string.pascalplural}}Domain;
using Common;
using Domain.Interfaces.Entities;
using Api.Common.Validators;
using Api.Interfaces.ServiceOperations.{{Parent.DomainName | string.pascalplural}};
using ServiceStack.FluentValidation;

namespace {{Parent.DomainName | string.pascalplural}}ApiHost.Services.{{Parent.DomainName | string.pascalplural}}
{
{{~if(Kind == "SEARCH")~}}
    internal class {{Name}}{{Parent.DomainName | string.pascalplural}}RequestValidator : AbstractValidator<{{Name}}{{Parent.DomainName | string.pascalplural}}Request>
{{~else~}}    
    internal class {{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidator : AbstractValidator<{{Name}}{{Parent.DomainName | string.pascalsingular}}Request>
{{~end~}}
    { 
{{~if(Kind == "GET" || Kind == "PUT" || Kind == "DELETE")~}}
        public {{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidator(IIdentifierFactory idFactory)
{{~else if(Kind == "SEARCH")~}}
        public {{Name}}{{Parent.DomainName | string.pascalplural}}RequestValidator(IHasSearchOptionsValidator searchOptionsValidator)
{{~else~}}
        public {{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidator()
{{~end~}}
        {
{{~if(Kind == "SEARCH")~}}
            Include(searchOptionsValidator);

{{~end~}}
{{~for field in Request.Field.Items~}}
    {{~if(field.Name == "Id")~}}
            RuleFor(dto => dto.Id)
                .IsEntityId(idFactory)
                .WithMessage(
                    Resources.AnyValidator_InvalidId);
    {{~else~}}
            RuleFor(dto => dto.{{field.Name}})
            .NotEmpty()
                .WithMessage(
                    Resources.{{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidator_Invalid{{field.Name}});
    {{~end~}}
{{~end~}}
        }
    }
}