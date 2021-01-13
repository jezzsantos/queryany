using System.IO;
using Storage.Interfaces;

namespace Storage
{
    public interface IBlobository
    {
        Blob Download(string containerName, string blobName, Stream stream);

        void Upload(string containerName, string blobName, string contentType, byte[] data);

        void DestroyAll(string containerName);
    }
}