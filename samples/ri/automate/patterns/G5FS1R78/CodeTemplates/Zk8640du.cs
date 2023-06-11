using System;
using System.Collections.Generic;
using System.Linq;
using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Resources;
using ApplicationServices.Interfaces;
using {{DomainName | string.pascalplural}}Application.Storage;
using {{DomainName | string.pascalplural}}Domain;
using Common;
using Domain.Interfaces.Entities;
using ServiceStack;
using {{DomainName | string.pascalsingular}} = Application.Interfaces.Resources.{{DomainName | string.pascalsingular}};

namespace {{DomainName | string.pascalplural}}Application
{
    public class {{DomainName | string.pascalplural}}Application : ApplicationBase, I{{DomainName | string.pascalplural}}Application
    {
        private readonly IIdentifierFactory idFactory;
        private readonly IRecorder recorder;
        private readonly I{{DomainName | string.pascalsingular}}Storage storage;

        public {{DomainName | string.pascalplural}}Application(IRecorder recorder, IIdentifierFactory idFactory,
            I{{DomainName | string.pascalsingular}}Storage storage)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            idFactory.GuardAgainstNull(nameof(idFactory));
            storage.GuardAgainstNull(nameof(storage));
            this.recorder = recorder;
            this.idFactory = idFactory;
            this.storage = storage;
        }

{{~for operation in ServiceOperation.Items~}}
    {{~if (operation.IsTestingOnly)~}}
#if TESTINGONLY
    {{~end~}}
    {{~if (operation.Kind == "SEARCH")~}}
        public SearchResults<Application.Interfaces.Resources.{{operation.Name}}{{DomainName | string.pascalsingular}}> {{operation.Name}}{{DomainName | string.pascalplural}}(ICurrentCaller caller{{for field in operation.Request.Field.Items}}, {{field.DataType}}{{if (field.IsOptional && field.DataType != "string")}}?{{end}} {{field.Name | string.camelcase}}{{end}}, SearchOptions searchOptions, GetOptions getOptions)
    {{~else~}}
        public Application.Interfaces.Resources.{{DomainName | string.pascalsingular}} {{operation.Name}}{{DomainName | string.pascalsingular}}(ICurrentCaller caller{{for field in operation.Request.Field.Items}}, {{field.DataType}}{{if (field.IsOptional && field.DataType != "string")}}?{{end}} {{field.Name | string.camelcase}}{{end}})
    {{~end~}}
        {
            throw new NotImplementedException();
        }
    {{~if (operation.IsTestingOnly)~}}
#endif
    {{~end~}}

{{~end~}}
    }

    public static class {{DomainName | string.pascalsingular}}ConversionExtensions
    {
        public static {{DomainName | string.pascalsingular}} To{{DomainName | string.pascalsingular}}(this ReadModels.{{DomainName | string.pascalsingular}} {{DomainName | string.camelsingular}})
        {
            var dto = {{DomainName | string.camelsingular}}.ConvertTo<{{DomainName | string.pascalsingular}}>();

            return dto;
        }

        public static {{DomainName | string.pascalsingular}} To{{DomainName | string.pascalsingular}}(this {{DomainName | string.pascalsingular}}Entity {{DomainName | string.camelsingular}})
        {
            var dto = {{DomainName | string.camelsingular}}.ConvertTo<{{DomainName | string.pascalsingular}}>();
            dto.Id = {{DomainName | string.camelsingular}}.Id;

            return dto;
        }
    }
}