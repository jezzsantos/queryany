using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public class GeneralEventingStorage<TAggregateRoot> : IEventingStorage<TAggregateRoot>
        where TAggregateRoot : IPersistableAggregateRoot
    {
        private readonly string containerName;
        private readonly IDomainFactory domainFactory;
        private readonly ILogger logger;
        private readonly IRepository repository;

        public GeneralEventingStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            this.logger = logger;
            this.repository = repository;
            this.domainFactory = domainFactory;
            this.containerName = typeof(TAggregateRoot).GetEntityNameSafe();
        }

        public event EventStreamStateChanged OnEventStreamStateChanged;

        public void DestroyAll()
        {
            this.repository.DestroyAll(this.containerName);
        }

        public TAggregateRoot Load(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            var streamName = GetEventStreamName(id);
            var eventContainerName = GetEventContainerName();

            var events = this.repository.Query(eventContainerName,
                Query.From<EventEntity>()
                    .Where(ee => ee.StreamName, ConditionOperator.EqualTo, streamName)
                    .OrderBy(ee => ee.LastPersistedAtUtc), this.domainFactory);
            if (!events.Any())
            {
                return RehydrateAggregateRoot(id, null);
            }

            var lastPersistedAtUtc = events.Last().LastPersistedAtUtc;
            var aggregate = RehydrateAggregateRoot(id, lastPersistedAtUtc);
            aggregate.LoadChanges(events);

            return aggregate;
        }

        public void Save(TAggregateRoot aggregate)
        {
            aggregate.GuardAgainstNull(nameof(aggregate));

            if (!aggregate.Id.HasValue() || aggregate.Id.IsEmpty())
            {
                throw new ResourceConflictException("The aggregate does not have an Identifier");
            }

            var events = aggregate.GetChanges();
            if (!events.Any())
            {
                return;
            }

            var eventContainerName = GetEventContainerName();

            events.ForEach(change =>
                this.repository.Add(eventContainerName, change, this.domainFactory));

            if (OnEventStreamStateChanged != null)
            {
                var streamName = events.First().StreamName;
                var changes = events
                    .Select(ToStateChange)
                    .ToList();
                try
                {
                    OnEventStreamStateChanged.Invoke(this, new EventStreamStateChangedArgs(streamName, changes));
                }
                catch (Exception ex)
                {
                    //Ignore exception and continue
                    this.logger.LogError(ex, $"Handling of event stream failed. Error was: {ex}");
                }
            }

            aggregate.ClearChanges();
        }

        private string GetEventStreamName(Identifier id)
        {
            return $"{this.containerName}_{id}";
        }

        private string GetEventContainerName()
        {
            return $"{this.containerName}_Events";
        }

        private static EventStreamStateChangeEvent ToStateChange(EventEntity @event)
        {
            var change = @event.ConvertTo<EventStreamStateChangeEvent>();
            change.Id = @event.Id;
            change.Type = @event.TypeName;
            change.Version = @event.Version;
            return change;
        }

        private TAggregateRoot RehydrateAggregateRoot(Identifier id, DateTime? lastPersistedAtUtc)
        {
            return (TAggregateRoot) this.domainFactory.RehydrateEntity(typeof(TAggregateRoot),
                new Dictionary<string, object>
                {
                    {nameof(IPersistableEntity.Id), id},
                    {nameof(IPersistableEntity.LastPersistedAtUtc), lastPersistedAtUtc}
                });
        }
    }
}