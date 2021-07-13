using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Common;
using ServiceStack.Configuration;

namespace Storage.Azure
{
    public class AzureQueueStorageRepository : IQueueository
    {
        private readonly string connectionString;
        private readonly Dictionary<string, bool> queueExistenceChecks = new Dictionary<string, bool>();
        private readonly IRecorder recorder;
        private QueueClient client;

        public AzureQueueStorageRepository(IRecorder recorder, string connectionString)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            this.connectionString = connectionString;
            this.recorder = recorder;
        }

        public void PopSingle(string queueName, Action<string> messageHandler)
        {
            queueName.GuardAgainstNullOrEmpty(nameof(queueName));
            messageHandler.GuardAgainstNull(nameof(messageHandler));

            var queue = EnsureQueue(queueName);

            var properties = queue.GetProperties().Value;
            if (properties.ApproximateMessagesCount > 0)
            {
                var message = FetchMessage();
                if (message.NotExists())
                {
                    this.recorder.TraceInformation("No message on queue: {Queue}", queue.Name);
                    return;
                }

                try
                {
                    messageHandler(message?.MessageText);
                }
                catch (Exception ex)
                {
                    ReturnMessageForNextPop(message);

                    this.recorder.Crash(CrashLevel.NonCritical, ex,
                        "Failed to handle last message: {MessageId} from queue: {Queue}",
                        message?.MessageId, queue.Name);
                    return;
                }

                MarkMessageAsHandled(message);
            }
        }

        public void DestroyAll(string queueName)
        {
            queueName.GuardAgainstNullOrEmpty(nameof(queueName));

            var queue = EnsureQueue(queueName);

            // NOTE: deleting the entire queue may take far too long (this method is only tenable in testing)
            queue.Delete();

            this.queueExistenceChecks.Remove(queueName);
        }

        public long Count(string queueName)
        {
            queueName.GuardAgainstNullOrEmpty(nameof(queueName));

            var queue = EnsureQueue(queueName);

            var properties = queue.GetProperties().Value;
            return properties.ApproximateMessagesCount;
        }

        public void Push(string queueName, string message)
        {
            queueName.GuardAgainstNullOrEmpty(nameof(queueName));
            message.GuardAgainstNullOrEmpty(nameof(message));

            var queue = EnsureQueue(queueName);

            try
            {
                var receipt = queue.SendMessage(message).Value;
                this.recorder.TraceInformation("Added message: {Message} to queue: {Queue}", receipt.MessageId,
                    queue.Name);
            }
            catch (Exception ex)
            {
                this.recorder.Crash(CrashLevel.NonCritical, ex, "Failed to push message: {Message} to queue: {Queue}",
                    message, queue.Name);
            }
        }

        public static AzureQueueStorageRepository FromSettings(IRecorder recorder, IAppSettings settings)
        {
            settings.GuardAgainstNull(nameof(settings));

            var accountKey = settings.GetString("Storage:AzureStorageAccountKey");
            var accountName = settings.GetString("Storage:AzureStorageAccountName");
            var connectionString = accountKey.HasValue()
                ? $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net"
                : "UseDevelopmentStorage=true";
            return new AzureQueueStorageRepository(recorder, connectionString);
        }

        private QueueMessage FetchMessage()
        {
            try
            {
                var retrievedMessage = this.client.ReceiveMessages(1).Value;
                return retrievedMessage.FirstOrDefault();
            }
            catch (Exception ex)
            {
                this.recorder.Crash(CrashLevel.NonCritical, ex, "Failed to pop last message from queue: {Queue}",
                    this.client.Name);
            }

            return null;
        }

        private void MarkMessageAsHandled(QueueMessage message)
        {
            try
            {
                this.client.DeleteMessage(message?.MessageId, message?.PopReceipt);
            }
            catch (Exception ex)
            {
                this.recorder.Crash(CrashLevel.NonCritical, ex,
                    "Failed to remove last message: {MessageId} from queue: {Queue}",
                    message?.MessageId, this.client.Name);
            }
        }

        private void ReturnMessageForNextPop(QueueMessage message)
        {
            Try.Safely(() =>
                this.client.UpdateMessage(message?.MessageId, message?.PopReceipt, (string) null, TimeSpan.Zero));
        }

        private QueueClient EnsureQueue(string name)
        {
            var queueName = name.SanitiseAndValidateStorageName();
            EnsureConnected(queueName);

            if (IsContainerExistenceCheckPerformed(queueName))
            {
                return this.client;
            }

            if (!this.client.Exists())
            {
                this.client.Create();
            }

            return this.client;
        }

        private void EnsureConnected(string queueName)
        {
            if (ObjectExtensions.Exists(this.client))
            {
                return;
            }

            this.client = new QueueClient(this.connectionString, queueName);
        }

        private bool IsContainerExistenceCheckPerformed(string queueName)
        {
            if (!this.queueExistenceChecks.ContainsKey(queueName))
            {
                this.queueExistenceChecks.Add(queueName, false);
            }

            if (this.queueExistenceChecks[queueName])
            {
                return true;
            }

            this.queueExistenceChecks[queueName] = true;

            return false;
        }
    }
}