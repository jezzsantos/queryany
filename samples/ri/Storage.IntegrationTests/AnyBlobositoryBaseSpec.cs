using System;
using System.IO;
using FluentAssertions;
using Funq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;

namespace Storage.IntegrationTests
{
    public class BloboInfo
    {
        public IBlobository Blobository { get; set; }

        public string ContainerName { get; set; }
    }

    public abstract class AnyBlobositoryBaseSpec
    {
        private static readonly ILogger Logger = new Logger<AnyRepositoryBaseSpec>(new NullLoggerFactory());
        private static Container container;
        private BloboInfo blobo;

        protected static void InitializeAllTests()
        {
            container = new Container();
            container.AddSingleton(Logger);
        }

        [TestInitialize]
        public void Initialize()
        {
            this.blobo = GetBlobository<TestRepositoryEntity>();
            this.blobo.Blobository.DestroyAll(this.blobo.ContainerName);
        }

        protected abstract BloboInfo GetBlobository<TQueryableEntity>()
            where TQueryableEntity : IQueryableEntity;

        [TestMethod]
        public void WhenDownloadWithNullContainerName_ThenThrows()
        {
            this.blobo.Blobository
                .Invoking(x => x.Download(null, "ablobname", new MemoryStream()))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenDownloadWithNullBlobName_ThenThrows()
        {
            this.blobo.Blobository
                .Invoking(x => x.Download(this.blobo.ContainerName, null, new MemoryStream()))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenDownloadWithNullStream_ThenThrows()
        {
            this.blobo.Blobository
                .Invoking(x => x.Download(this.blobo.ContainerName, "ablobname", null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenDownloadAndNotExists_ThenReturnsNull()
        {
            var result = this.blobo.Blobository.Download(this.blobo.ContainerName, "ablobname", new MemoryStream());

            result.Should().BeNull();
        }

        [TestMethod]
        public void WhenDownloadAndExists_ThenReturnsBlob()
        {
            var data = new byte[] {0x00, 0x01, 0x02};
            this.blobo.Blobository.Upload(this.blobo.ContainerName, "ablobname", "acontenttype", data);
            using (var stream = new MemoryStream())
            {
                var result = this.blobo.Blobository.Download(this.blobo.ContainerName, "ablobname", stream);

                result.ContentType.Should().Be("acontenttype");
                stream.Position = 0;
                stream.ReadFully().Should().BeEquivalentTo(data);
            }
        }

        [TestMethod]
        public void WhenUploadAndContainerNameIsNull_ThenThrows()
        {
            this.blobo.Blobository
                .Invoking(x => x.Upload(null, "ablobname", "acontenttype", new byte[0]))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUploadAndBlobNameIsNull_ThenThrows()
        {
            this.blobo.Blobository
                .Invoking(x => x.Upload(this.blobo.ContainerName, null, "acontenttype", new byte[0]))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUploadAndContentTypeIsNull_ThenThrows()
        {
            this.blobo.Blobository
                .Invoking(x => x.Upload(this.blobo.ContainerName, "ablobname", null, new byte[0]))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUploadAndDataIsNull_ThenThrows()
        {
            this.blobo.Blobository
                .Invoking(x => x.Upload(this.blobo.ContainerName, "ablobname", "acontenttype", null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUploadAndExists_ThenOverwrites()
        {
            var data = new byte[] {0x00, 0x01, 0x02};
            this.blobo.Blobository.Upload(this.blobo.ContainerName, "ablobname", "acontenttype", data);
            var newData = new byte[] {0x03, 0x04, 0x05};

            this.blobo.Blobository.Upload(this.blobo.ContainerName, "ablobname", "anewcontenttype", newData);

            using (var stream = new MemoryStream())
            {
                var result = this.blobo.Blobository.Download(this.blobo.ContainerName, "ablobname", stream);

                result.ContentType.Should().Be("anewcontenttype");
                stream.Position = 0;
                stream.ReadFully().Should().BeEquivalentTo(newData);
            }
        }

        [TestMethod]
        public void WhenUploadAndNotExists_ThenAddsNewBLob()
        {
            var data = new byte[] {0x00, 0x01, 0x02};
            this.blobo.Blobository.Upload(this.blobo.ContainerName, "ablobname", "acontenttype", data);

            using (var stream = new MemoryStream())
            {
                var result = this.blobo.Blobository.Download(this.blobo.ContainerName, "ablobname", stream);

                result.ContentType.Should().Be("acontenttype");
                stream.Position = 0;
                stream.ReadFully().Should().BeEquivalentTo(data);
            }
        }
    }
}