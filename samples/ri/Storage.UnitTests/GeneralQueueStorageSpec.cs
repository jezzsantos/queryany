using System;
using Common;
using FluentAssertions;
using Moq;
using ServiceStack;
using Xunit;

namespace Storage.UnitTests
{
    [Trait("Category", "Unit")]
    public class GeneralQueueStorageSpec
    {
        private readonly Mock<IQueueository> queueository;
        private readonly GeneralQueueStorage<TestQueueMessage> storage;

        public GeneralQueueStorageSpec()
        {
            var recorder = new Mock<IRecorder>();
            this.queueository = new Mock<IQueueository>();
            this.storage = new GeneralQueueStorage<TestQueueMessage>(recorder.Object, this.queueository.Object);
        }

        [Fact]
        public void WhenPopSingleAndNotExists_ThenDoesNotHandle()
        {
            this.queueository.Setup(repo => repo.PopSingle("aqueuename", It.IsAny<Action<string>>()))
                .Callback((string name, Action<string> handler) => { });

            var wasCalled = false;
            this.storage.PopSingle(msg => { wasCalled = true; });

            wasCalled.Should().BeFalse();
            this.queueository.Verify(repo => repo.PopSingle("aqueuename", It.IsAny<Action<string>>()));
        }

        [Fact]
        public void WhenPush_ThenPushesToRepository()
        {
            var message = new TestQueueMessage
            {
                ABooleanValue = true,
                ADoubleValue = 9,
                AStringProperty = "avalue"
            };
            this.storage.Push(message);

            this.queueository.Verify(repo => repo.Push("aqueuename", message.ToJson()));
        }

        [Fact]
        public void WhenCount_ThenGetsCountFromRepo()
        {
            this.storage.Count();

            this.queueository.Verify(repo => repo.Count("aqueuename"));
        }

        [Fact]
        public void WhenDestroyAll_ThenGetsCountFromRepo()
        {
            this.storage.DestroyAll();

            this.queueository.Verify(repo => repo.DestroyAll("aqueuename"));
        }
    }
}