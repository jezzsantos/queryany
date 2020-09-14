namespace Storage.Interfaces.ReadModels
{
    public interface IReadModelCheckpointStore
    {
        long LoadCheckpoint(string streamName);

        void SaveCheckpoint(string streamName, long position);
    }
}