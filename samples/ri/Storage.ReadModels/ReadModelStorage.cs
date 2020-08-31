using System;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.ReadModels
{
    public class ReadModelStorage<TAggregateRoot> : IReadModelStorage
        where TAggregateRoot : IPersistableAggregateRoot
    {
        private readonly ICheckpointStore checkpointStore;
        private readonly ICommandStorage<TAggregateRoot> commandStorage;
        private readonly ILogger logger;

        public ReadModelStorage(ILogger logger, ICommandStorage<TAggregateRoot> commandStorage,
            ICheckpointStore checkpointStore)
        {
            logger.GuardAgainstNull(nameof(logger));
            commandStorage.GuardAgainstNull(nameof(commandStorage));
            checkpointStore.GuardAgainstNull(nameof(checkpointStore));

            this.logger = logger;
            this.commandStorage = commandStorage;
            this.checkpointStore = checkpointStore;
        }

        public long ReadCheckpoint(string streamName)
        {
            return this.checkpointStore.LoadCheckpoint(streamName);
        }

        public void WriteCheckPoint(string streamName, in long checkpoint)
        {
            this.checkpointStore.SaveCheckpoint(streamName, checkpoint);
        }

        public void WriteEvent(object @event)
        {
            //TODO: save the event to commandStorage
            throw new NotImplementedException();
        }
    }
}