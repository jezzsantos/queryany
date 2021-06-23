using System;

namespace Storage
{
    public interface IQueueository
    {
        void Push(string queueName, string message);

        void PopSingle(string queueName, Action<string> messageHandler);

        void DestroyAll(string queueName);

        long Count(string queueName);
    }
}