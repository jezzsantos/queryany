namespace Storage.Interfaces
{
    public interface IReadModelCheckpointStore
    {
        long LoadCheckpoint(string streamName);

        void SaveCheckpoint(string streamName, long position);
    }
}