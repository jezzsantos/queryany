using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
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

        public InProcessReadModelProjectionSubscription(IRecorder recorder, IIdentifierFactory idFactory,
            IChangeEventMigrator migrator, IDomainFactory domainFactory, IRepository repository,
            IEnumerable<IReadModelProjection> projections,
            params IEventNotifyingStorage[] eventingStorages) : base(recorder, eventingStorages)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            idFactory.GuardAgainstNull(nameof(idFactory));
            migrator.GuardAgainstNull(nameof(migrator));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));
            projections.GuardAgainstNull(nameof(projections));
            var checkpointReadModel = new ReadModelCheckpointStore(recorder, idFactory, domainFactory, repository);
            this.projector = new ReadModelProjector(recorder, checkpointReadModel, migrator, projections?.ToArray());
        }

        protected override void HandleStreamEvents(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            this.projector.WriteEventStream(streamName, eventStream);
        }
    }
}