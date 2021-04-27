using System.IO;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Storage.Interfaces;

namespace Storage
{
    public class GeneralBlobStorage : IBlobStorage
    {
        private readonly IBlobository blobository;
        private readonly string containerName;
        private readonly ILogger logger;

        public GeneralBlobStorage(ILogger logger, string containerName, IBlobository blobository)
        {
            logger.GuardAgainstNull(nameof(logger));
            blobository.GuardAgainstNull(nameof(blobository));
            containerName.GuardAgainstNull(nameof(containerName));
            this.logger = logger;
            this.blobository = blobository;
            this.containerName = containerName;
        }

        public Blob Get(string blobName, MemoryStream stream)
        {
            blobName.GuardAgainstNullOrEmpty(nameof(blobName));
            stream.GuardAgainstNull(nameof(stream));

            var blob = this.blobository.Download(this.containerName, blobName, stream);
            this.logger.LogDebug("Blob {Name} was retrieved from the {Container} blobository", blobName,
                this.containerName);

            return blob;
        }

        public void Save(string blobName, string contentType, byte[] data)
        {
            blobName.GuardAgainstNullOrEmpty(nameof(blobName));
            contentType.GuardAgainstNullOrEmpty(nameof(contentType));
            data.GuardAgainstNull(nameof(data));

            this.blobository.Upload(this.containerName, blobName, contentType, data);
            this.logger.LogDebug("Blob {Name} was saved to the {Container} blobository", blobName, this.containerName);
        }

        public void DestroyAll()
        {
            this.blobository.DestroyAll(this.containerName);
        }
    }
}