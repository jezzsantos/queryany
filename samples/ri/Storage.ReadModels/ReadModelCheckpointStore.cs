using System.Linq;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.ReadModels
{
    public class ReadModelCheckpointStore : IReadModelCheckpointStore
    {
        public const long DefaultCheckpointPosition = 1;
        private readonly ICommandStorage<Checkpoint> commandStorage;
        private readonly IIdentifierFactory idFactory;
        private readonly ILogger logger;
        private readonly IQueryStorage<Checkpoint> queryStorage;

        public ReadModelCheckpointStore(ILogger logger, IIdentifierFactory idFactory,
            ICommandStorage<Checkpoint> commandStorage,
            IQueryStorage<Checkpoint> queryStorage)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            queryStorage.GuardAgainstNull(nameof(queryStorage));
            commandStorage.GuardAgainstNull(nameof(commandStorage));
            this.logger = logger;
            this.idFactory = idFactory;
            this.queryStorage = queryStorage;
            this.commandStorage = commandStorage;
        }

        public long LoadCheckpoint(string streamName)
        {
            var checkpoint = GetCheckpoint(streamName);
            return checkpoint == null
                ? DefaultCheckpointPosition
                : checkpoint.Position;
        }

        public void SaveCheckpoint(string streamName, long position)
        {
            var checkpoint = GetCheckpoint(streamName);
            if (checkpoint == null)
            {
                checkpoint = new Checkpoint
                {
                    Position = position,
                    StreamName = streamName
                };
                checkpoint.SetIdentifier(this.idFactory);
            }
            else
            {
                checkpoint.Position = position;
            }

            this.commandStorage.Upsert(checkpoint);

            this.logger.LogDebug("Saved checkpoint {StreamName} to position: {Position}", streamName,
                position);
        }

        private Checkpoint GetCheckpoint(string streamName)
        {
            var checkpoint = this.queryStorage
                .Query(Query.From<Checkpoint>().Where(cp => cp.StreamName, ConditionOperator.EqualTo, streamName))
                .Results.FirstOrDefault();
            return checkpoint;
        }
    }
}