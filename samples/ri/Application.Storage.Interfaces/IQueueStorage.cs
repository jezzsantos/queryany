using System;

namespace Application.Storage.Interfaces
{
    public interface IQueueStorage<TMessage> where TMessage : new()
    {
        void Push(TMessage message);

        void PopSingle(Action<TMessage> onMessageReceived);

        long Count();

        void DestroyAll();
    }
}