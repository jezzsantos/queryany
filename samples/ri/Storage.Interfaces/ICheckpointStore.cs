namespace Storage.Interfaces
{
    public interface ICheckpointStore
    {
        long LoadCheckpoint(string streamName);

        void SaveCheckpoint(string streamName, long checkpoint);
    }
}