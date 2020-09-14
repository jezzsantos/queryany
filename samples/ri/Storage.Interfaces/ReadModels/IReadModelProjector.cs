using System.Collections.Generic;

namespace Storage.Interfaces.ReadModels
{
    public interface IReadModelProjector
    {
        void WriteEventStream(string streamName, List<EventStreamStateChangeEvent> eventStream);
    }
}