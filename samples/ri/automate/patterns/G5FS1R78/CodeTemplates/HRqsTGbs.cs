using System;
using Api.Interfaces.ServiceOperations.{{Parent.DomainName | string.pascalplural}};
using {{Parent.DomainName | string.pascalplural}}ApiHost.Services.{{Parent.DomainName | string.pascalplural}};
using {{Parent.DomainName | string.pascalplural}}Domain;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using ServiceStack.FluentValidation;
using UnitTesting.Common;
using Xunit;

namespace {{Parent.DomainName | string.pascalplural}}ApiHost.UnitTests.Services.{{Parent.DomainName | string.pascalplural}}
{
    [Trait("Category", "Unit")]
{{~if(Kind == "SEARCH")~}}
    public class {{Name}}{{Parent.DomainName | string.pascalplural}}RequestValidatorSpec
{{~else~}}    
    public class {{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidatorSpec
{{~end~}}
    {
{{~if(Kind == "SEARCH")~}}
        private readonly {{Name}}{{Parent.DomainName | string.pascalplural}}Request dto;
        private readonly {{Name}}{{Parent.DomainName | string.pascalplural}}RequestValidator validator;

        public {{Name}}{{Parent.DomainName | string.pascalplural}}RequestValidatorSpec()
{{~else~}}
        private readonly {{Name}}{{Parent.DomainName | string.pascalsingular}}Request dto;
        private readonly {{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidator validator;

        public {{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidatorSpec()
{{~end~}}
        {
{{~if (Kind == "GET" || Kind == "PUT" || Kind == "DELETE")~}}
            this.validator = new {{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidator(Mock.Of<IIdentifierFactory>(idf=> idf.IsValid(It.IsAny<Identifier>()) == true));
            this.dto = new {{Name}}{{Parent.DomainName | string.pascalsingular}}Request
{{~else if (Kind == "SEARCH")~}}
            this.validator = new {{Name}}{{Parent.DomainName | string.pascalplural}}RequestValidator(new HasSearchOptionsValidator(new HasGetOptionsValidator()));
            this.dto = new {{Name}}{{Parent.DomainName | string.pascalplural}}Request
{{~else~}}
            this.validator = new {{Name}}{{Parent.DomainName | string.pascalsingular}}RequestValidator();
            this.dto = new {{Name}}{{Parent.DomainName | string.pascalsingular}}Request
{{~end~}}
            {
{{~for field in Request.Field.Items~}}
    {{~if(field.Name == "Id")~}}
                Id = "anid",
    {{~else~}}
        {{~if(field.IsOptional==false)~}}
                {{field.Name}} = {{dataType = field.DataType; case dataType; when "string"; "\"a"+field.Name | string.downcase + "\""; when "int"; 1; when "decimal"; "1M"; when "bool"; "false"; when "DateTime"; "DateTime.UtcNow"; end}},
        {{~end~}}
    {{~end~}}
{{~end~}}
            };
        }

        [Fact]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }
    }
}