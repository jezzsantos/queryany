using System;
using Application.Storage.Interfaces;
using Common;
using QueryAny;
using ServiceStack;

namespace Storage
{
    public class GeneralQueueStorage<TMessage> : IQueueStorage<TMessage> where TMessage : new()
    {
        private readonly string queueName;
        private readonly IQueueository queueository;
        private readonly IRecorder recorder;

        public GeneralQueueStorage(IRecorder recorder, IQueueository queueository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            queueository.GuardAgainstNull(nameof(queueository));
            this.recorder = recorder;
            this.queueository = queueository;
            this.queueName = typeof(TMessage).GetEntityNameSafe();
        }

        public void Push(TMessage message)
        {
            message.GuardAgainstNull(nameof(message));

            var messageJson = message.ToJson();

            this.queueository.Push(this.queueName, messageJson);
            this.recorder.TraceDebug("Message {Message} was added to the {Queue} queueository", messageJson,
                this.queueName);
        }

        public void PopSingle(Action<TMessage> onMessageReceived)
        {
            onMessageReceived.GuardAgainstNull(nameof(onMessageReceived));

            TMessage message;
            this.queueository.PopSingle(this.queueName, text =>
            {
                if (text.HasValue())
                {
                    message = text.FromJson<TMessage>();
                    onMessageReceived(message);

                    this.recorder.TraceDebug("Message {Text} was removed from the {Queue} queueository", text,
                        this.queueName);
                }
            });
        }

        public long Count()
        {
            return this.queueository.Count(this.queueName);
        }

        public void DestroyAll()
        {
            this.queueository.DestroyAll(this.queueName);
        }
    }
}