using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage;
using Storage.Interfaces;
using Storage.Interfaces.ReadModels;
using Storage.ReadModels;

namespace InfrastructureServices.Eventing.ReadModels
{
    /// <summary>
    ///     Defines a subscription that connects directly to one or more <see cref="IEventStreamStorage{TAggregateRoot}" />
    ///     instances, to listen to raised events, that are then relayed to listening projections that build read models.
    /// </summary>
    public class InProcessReadModelProjectionSubscription : EventStreamHandlerBase, IReadModelProjectionSubscription
    {
        private readonly IReadModelProjector projector;

        public InProcessReadModelProjectionSubscription(ILogger logger, IIdentifierFactory idFactory,
            IChangeEventMigrator migrator, IDomainFactory domainFactory, IRepository repository,
            IEnumerable<IReadModelProjection> projections,
            params IEventNotifyingStorage[] eventingStorages) : base(logger, eventingStorages)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            migrator.GuardAgainstNull(nameof(migrator));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));
            projections.GuardAgainstNull(nameof(projections));
            var checkpointReadModel = new ReadModelCheckpointStore(logger, idFactory, domainFactory, repository);
            this.projector = new ReadModelProjector(logger, checkpointReadModel, migrator, projections?.ToArray());
        }

        protected override void HandleStreamEvents(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            this.projector.WriteEventStream(streamName, eventStream);
        }
    }
}