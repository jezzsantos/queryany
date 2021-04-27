using System.IO;
using Domain.Interfaces;
using Storage.Interfaces;

namespace Storage
{
    public class GeneralBlobStorage : IBlobStorage
    {
        private readonly IBlobository blobository;
        private readonly string containerName;
        private readonly IRecorder recorder;

        public GeneralBlobStorage(IRecorder recorder, string containerName, IBlobository blobository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            blobository.GuardAgainstNull(nameof(blobository));
            containerName.GuardAgainstNull(nameof(containerName));
            this.recorder = recorder;
            this.blobository = blobository;
            this.containerName = containerName;
        }

        public Blob Get(string blobName, MemoryStream stream)
        {
            blobName.GuardAgainstNullOrEmpty(nameof(blobName));
            stream.GuardAgainstNull(nameof(stream));

            var blob = this.blobository.Download(this.containerName, blobName, stream);
            this.recorder.TraceDebug("Blob {Name} was retrieved from the {Container} blobository", blobName,
                this.containerName);

            return blob;
        }

        public void Save(string blobName, string contentType, byte[] data)
        {
            blobName.GuardAgainstNullOrEmpty(nameof(blobName));
            contentType.GuardAgainstNullOrEmpty(nameof(contentType));
            data.GuardAgainstNull(nameof(data));

            this.blobository.Upload(this.containerName, blobName, contentType, data);
            this.recorder.TraceDebug("Blob {Name} was saved to the {Container} blobository", blobName,
                this.containerName);
        }

        public void DestroyAll()
        {
            this.blobository.DestroyAll(this.containerName);
        }
    }
}