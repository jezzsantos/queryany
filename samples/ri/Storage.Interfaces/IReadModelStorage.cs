namespace Storage.Interfaces
{
    public interface IReadModelStorage
    {
        long ReadCheckpoint(string streamName);

        void WriteCheckPoint(string streamName, in long checkpoint);

        void WriteEvent(object @event);
    }
}