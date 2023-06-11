using System;
using System.Collections.Generic;
using Application.Storage.Interfaces.ReadModels;
using {{DomainName | string.pascalplural}}Application.ReadModels;
using {{DomainName | string.pascalplural}}Domain;
using Common;
using Domain.Interfaces.Entities;
using Storage;

namespace {{DomainName | string.pascalplural}}Storage
{
    public class {{DomainName | string.pascalsingular}}EntityReadModelProjection : IReadModelProjection
    {
        private readonly IReadModelStorage<{{DomainName | string.pascalsingular}}> {{DomainName | string.camelsingular}}Storage;
        private readonly IRecorder recorder;

        public {{DomainName | string.pascalsingular}}EntityReadModelProjection(IRecorder recorder, IRepository repository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            repository.GuardAgainstNull(nameof(repository));

            this.recorder = recorder;
            this.{{DomainName | string.camelsingular}}Storage = new GeneralReadModelStorage<{{DomainName | string.pascalsingular}}>(recorder, repository);
        }

        public Type EntityType => typeof({{DomainName | string.pascalsingular}}Entity);

        public bool Project(IChangeEvent originalEvent)
        {
            switch (originalEvent)
            {
                case Events.{{DomainName | string.pascalsingular}}.Created e:
                    this.{{DomainName | string.camelsingular}}Storage.Create(e.EntityId.ToIdentifier());
                    break;

                default:
                    this.recorder.TraceDebug($"Unknown entity type '{originalEvent.GetType().Name}'");
                    return false;
            }

            return true;
        }
    }
}