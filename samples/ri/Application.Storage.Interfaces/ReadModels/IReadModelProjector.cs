using System.Collections.Generic;

namespace Application.Storage.Interfaces.ReadModels
{
    public interface IReadModelProjector
    {
        void WriteEventStream(string streamName, List<EventStreamStateChangeEvent> eventStream);
    }
}