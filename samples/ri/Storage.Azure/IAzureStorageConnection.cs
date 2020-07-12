namespace Storage.Azure
{
    public interface IAzureStorageConnection
    {
        IRepository Open();
    }
}