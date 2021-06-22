using System.IO;
using Common;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class GeneralBlobStorageSpec
    {
        private readonly Mock<IBlobository> blobository;
        private readonly GeneralBlobStorage storage;

        public GeneralBlobStorageSpec()
        {
            var recorder = new Mock<IRecorder>();
            this.blobository = new Mock<IBlobository>();
            this.storage = new GeneralBlobStorage(recorder.Object, "acontainername", this.blobository.Object);
        }

        [TestMethod]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            var stream = new MemoryStream();
            this.blobository.Setup(blo => blo.Download(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Returns((Blob) null);

            var result = this.storage.Get("ablobname", stream);

            result.Should().BeNull();
            this.blobository.Verify(blo => blo.Download("acontainername", "ablobname", stream));
        }

        [TestMethod]
        public void WhenGet_ThenDownloadsFromRepo()
        {
            var stream = new MemoryStream();
            this.blobository.Setup(blo => blo.Download(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Returns(new Blob {ContentType = "acontenttype"});

            var result = this.storage.Get("ablobname", stream);

            result.ContentType.Should().Be("acontenttype");
            this.blobository.Verify(blo => blo.Download("acontainername", "ablobname", stream));
        }

        [TestMethod]
        public void WhenSave_ThenUploadsToRepo()
        {
            var data = new byte[0];
            this.storage.Save("ablobname", "acontenttype", data);

            this.blobository.Verify(blo => blo.Upload("acontainername", "ablobname", "acontenttype", data));
        }
    }
}