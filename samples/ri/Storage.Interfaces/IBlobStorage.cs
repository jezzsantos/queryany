using System.IO;

namespace Storage.Interfaces
{
    public interface IBlobStorage
    {
        Blob Get(string blobName, MemoryStream stream);

        void Save(string blobName, string contentType, byte[] data);

        void DestroyAll();
    }

    public class Blob
    {
        public string ContentType { get; set; }
    }
}