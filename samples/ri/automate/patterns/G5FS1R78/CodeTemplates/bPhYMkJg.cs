using System;
using {{DomainName | string.pascalplural}}Domain.Properties;
using Common;
using Domain.Interfaces.Entities;
using QueryAny;

namespace {{DomainName | string.pascalplural}}Domain
{
    [EntityName("{{DomainName | string.pascalsingular}}")]
    public class {{DomainName | string.pascalsingular}}Entity : AggregateRootBase
    {
        public {{DomainName | string.pascalsingular}}Entity(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory,
            {{DomainName | string.pascalplural}}Domain.Events.{{DomainName | string.pascalsingular}}.Created.Create)
        {
        }

        private {{DomainName | string.pascalsingular}}Entity(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder,
            idFactory,
            identifier)
        {
        }

        protected override void OnStateChanged(IChangeEvent @event)
        {
            switch (@event)
            {
                case Events.{{DomainName | string.pascalsingular}}.Created _:
                    break;

                default:
                    throw new InvalidOperationException($"Unknown event {@event.GetType()}");
            }
        }


        protected override bool EnsureValidState()
        {
            var isValid = base.EnsureValidState();

            return isValid;
        }

        public static AggregateRootFactory<{{DomainName | string.pascalsingular}}Entity> Rehydrate()
        {
            return (identifier, container, rehydratingProperties) => new {{DomainName | string.pascalsingular}}Entity(container.Resolve<IRecorder>(),
                container.Resolve<IIdentifierFactory>(), identifier);
        }
    }
}