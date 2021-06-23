using System;
using Common;
using FluentAssertions;
using Funq;
using QueryAny;
using ServiceStack;
using Xunit;

namespace Storage.IntegrationTests
{
    public class QueueoInfo
    {
        public IQueueository Queueository { get; set; }

        public string QueueName { get; set; }
    }

    public abstract class AnyQueueositoryBaseSpec
    {
        private static readonly IRecorder Recorder = NullRecorder.Instance;
        private readonly QueueoInfo queueo;

        protected AnyQueueositoryBaseSpec(IQueueository queueository)
        {
            var container = new Container();
            container.AddSingleton(Recorder);
            this.queueo = new QueueoInfo
                {Queueository = queueository, QueueName = typeof(TestRepositoryEntity).GetEntityNameSafe()};
            this.queueo.Queueository.DestroyAll(this.queueo.QueueName);
        }

        [Fact]
        public void WhenPopSingleWithNullQueueName_ThenThrows()
        {
            this.queueo.Queueository
                .Invoking(x => x.PopSingle(null, s => { }))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenPopSingleWithNullHandler_ThenThrows()
        {
            this.queueo.Queueository
                .Invoking(x => x.PopSingle(this.queueo.QueueName, null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenPopSingleAndNoMessage_ThenDoesNothing()
        {
            var wasCalled = false;
            this.queueo.Queueository.PopSingle(this.queueo.QueueName, s => { wasCalled = true; });

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void WhenPopSingleAndMessageExists_ThenExecutesHandler()
        {
            this.queueo.Queueository.Push(this.queueo.QueueName, "amessage");

            string message = null;
            this.queueo.Queueository.PopSingle(this.queueo.QueueName, msg => { message = msg; });

            message.Should().Be("amessage");

            this.queueo.Queueository.Count(this.queueo.QueueName).Should().Be(0);
        }

        [Fact]
        public void WhenPopSingleAndMessageExistsAndHandlerFails_ThenLeavesMessageOnQueue()
        {
            this.queueo.Queueository.Push(this.queueo.QueueName, "amessage");

            this.queueo.Queueository.PopSingle(this.queueo.QueueName, msg => throw new Exception());

            this.queueo.Queueository.Count(this.queueo.QueueName).Should().Be(1);

            string remainingMessage = null;
            this.queueo.Queueository.PopSingle(this.queueo.QueueName, msg => { remainingMessage = msg; });

            remainingMessage.Should().Be("amessage");
        }
    }
}