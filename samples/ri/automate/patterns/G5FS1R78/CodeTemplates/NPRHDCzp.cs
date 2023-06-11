using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Application.Interfaces.Storage;
using Application.Storage.Interfaces;
using {{DomainName | string.pascalplural}}Application.ReadModels;
using {{DomainName | string.pascalplural}}Application.Storage;
using {{DomainName | string.pascalplural}}Domain;
using Common;
using Domain.Interfaces.Entities;
using QueryAny;
using Storage;

namespace {{DomainName | string.pascalplural}}Storage
{
    public class {{DomainName | string.pascalsingular}}Storage : I{{DomainName | string.pascalsingular}}Storage
    {
        private readonly IEventStreamStorage<{{DomainName | string.pascalsingular}}Entity> {{DomainName | string.camelsingular}}EventStreamStorage;
        private readonly IQueryStorage<{{DomainName | string.pascalsingular}}> {{DomainName | string.camelsingular}}QueryStorage;

        public {{DomainName | string.pascalsingular}}Storage(IRecorder recorder, IDomainFactory domainFactory,
            IEventStreamStorage<{{DomainName | string.pascalsingular}}Entity> eventStreamStorage,
            IRepository repository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            eventStreamStorage.GuardAgainstNull(nameof(eventStreamStorage));
            repository.GuardAgainstNull(nameof(repository));

            this.{{DomainName | string.camelsingular}}QueryStorage = new GeneralQueryStorage<{{DomainName | string.pascalsingular}}>(recorder, domainFactory, repository);
            this.{{DomainName | string.camelsingular}}EventStreamStorage = eventStreamStorage;
        }

        public {{DomainName | string.pascalsingular}}Storage(IEventStreamStorage<{{DomainName | string.pascalsingular}}Entity> {{DomainName | string.camelsingular}}EventStreamStorage,
            IQueryStorage<{{DomainName | string.pascalsingular}}> {{DomainName | string.camelsingular}}QueryStorage)
        {
            {{DomainName | string.camelsingular}}QueryStorage.GuardAgainstNull(nameof({{DomainName | string.camelsingular}}QueryStorage));
            {{DomainName | string.camelsingular}}EventStreamStorage.GuardAgainstNull(nameof({{DomainName | string.camelsingular}}EventStreamStorage));
            this.{{DomainName | string.camelsingular}}QueryStorage = {{DomainName | string.camelsingular}}QueryStorage;
            this.{{DomainName | string.camelsingular}}EventStreamStorage = {{DomainName | string.camelsingular}}EventStreamStorage;
        }

        public {{DomainName | string.pascalsingular}}Entity Load(Identifier id)
        {
            return this.{{DomainName | string.camelsingular}}EventStreamStorage.Load(id);
        }

        public {{DomainName | string.pascalsingular}}Entity Save({{DomainName | string.pascalsingular}}Entity {{DomainName | string.camelsingular}})
        {
            this.{{DomainName | string.camelsingular}}EventStreamStorage.Save({{DomainName | string.camelsingular}});
            return {{DomainName | string.camelsingular}};
        }

    }
}