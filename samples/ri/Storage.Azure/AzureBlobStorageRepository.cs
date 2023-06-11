using System.Collections.Generic;
using System.IO;
using System.Linq;
using Application.Storage.Interfaces;
using Common;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using ServiceStack.Configuration;

namespace Storage.Azure
{
    public class AzureBlobStorageRepository : IBlobository
    {
        private readonly string connectionString;
        private readonly Dictionary<string, bool> containerExistenceChecks = new Dictionary<string, bool>();

        private CloudBlobClient client;

        public AzureBlobStorageRepository(string connectionString)
        {
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            this.connectionString = connectionString;
        }

        public Blob Download(string containerName, string blobName, Stream stream)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            blobName.GuardAgainstNullOrEmpty(nameof(blobName));
            stream.GuardAgainstNull(nameof(stream));

            var container = EnsureContainer(containerName);

            var blob = container.GetBlockBlobReference(blobName);
            if (!blob.Exists())
            {
                return null;
            }

            blob.DownloadToStream(stream);

            return new Blob
            {
                ContentType = blob.Properties.ContentType
            };
        }

        public void Upload(string containerName, string blobName, string contentType, byte[] data)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            blobName.GuardAgainstNullOrEmpty(nameof(blobName));
            contentType.GuardAgainstNullOrEmpty(nameof(contentType));
            data.GuardAgainstNull(nameof(data));

            var container = EnsureContainer(containerName);

            var blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = contentType;
            blob.UploadFromByteArray(data, 0, data.Length);
            blob.SetProperties();
        }

        public void DestroyAll(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            var container = EnsureContainer(containerName);

            // NOTE: deleting the entire container may take far too long (this method is only tenable in testing)
            var blobs = container.ListBlobs().ToList();
            blobs.ForEach(item =>
            {
                var blob = container.GetBlockBlobReference(item.Uri.ToString());
                blob.DeleteIfExists();
            });
        }

        public static AzureBlobStorageRepository FromSettings(IAppSettings settings)
        {
            settings.GuardAgainstNull(nameof(settings));

            var accountKey = settings.GetString("Storage:AzureStorageAccountKey");
            var accountName = settings.GetString("Storage:AzureStorageAccountName");
            var connectionString = accountKey.HasValue()
                ? $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net"
                : "UseDevelopmentStorage=true";
            return new AzureBlobStorageRepository(connectionString);
        }

        private CloudBlobContainer EnsureContainer(string name)
        {
            var containerName = name.SanitiseAndValidateStorageName();
            EnsureConnected();
            var container = this.client.GetContainerReference(containerName);

            if (IsContainerExistenceCheckPerformed(containerName))
            {
                return container;
            }

            if (!container.Exists())
            {
                container.Create();
            }

            return container;
        }

        private void EnsureConnected()
        {
            if (this.client != null)
            {
                return;
            }

            var account = CloudStorageAccount.Parse(this.connectionString);
            this.client = account.CreateCloudBlobClient();
        }

        private bool IsContainerExistenceCheckPerformed(string containerName)
        {
            this.containerExistenceChecks.TryAdd(containerName, false);

            if (this.containerExistenceChecks[containerName])
            {
                return true;
            }

            this.containerExistenceChecks[containerName] = true;

            return false;
        }
    }
}