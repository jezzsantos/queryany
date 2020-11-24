using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;
using Storage.Interfaces.ReadModels;

namespace InfrastructureServices.Eventing.ReadModels
{
    /// <summary>
    ///     Defines a subscription that connects directly to one or more <see cref="IEventStreamStorage{TAggregateRoot}" />
    ///     instances, to listen to raised events, that are then relayed to listening projections that build read models.
    /// </summary>
    public class InProcessReadModelProjectionSubscription : EventStreamHandlerBase, IReadModelProjectionSubscription
    {
        private readonly IReadModelProjector projector;

        public InProcessReadModelProjectionSubscription(ILogger logger, IReadModelProjector readModelProjector,
            params IEventNotifyingStorage[] eventingStorages) : base(logger, eventingStorages)
        {
            readModelProjector.GuardAgainstNull(nameof(readModelProjector));
            this.projector = readModelProjector;
        }

        protected override void HandleStreamEvents(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            this.projector.WriteEventStream(streamName, eventStream);
        }
    }
}