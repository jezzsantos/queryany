using System.Collections.Generic;

namespace Storage.Interfaces
{
    public interface IReadModelProjector
    {
        void WriteEventStream(string streamName, List<EventStreamStateChangeEvent> eventStream);
    }
}