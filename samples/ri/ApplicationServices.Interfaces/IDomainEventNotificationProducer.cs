using System.Collections.Generic;
using Application.Storage.Interfaces;

namespace ApplicationServices.Interfaces
{
    public interface IDomainEventNotificationProducer
    {
        void WriteEventStream(string streamName, List<EventStreamStateChangeEvent> eventStream);
    }
}