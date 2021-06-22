using System.Linq;
using Common;
using Domain.Interfaces.Entities;
using QueryAny;
using Storage.Interfaces.ReadModels;

namespace Storage.ReadModels
{
    public sealed class ReadModelCheckpointStore : IReadModelCheckpointStore
    {
        public const long StartingCheckpointPosition = 1;
        private readonly IDomainFactory domainFactory;
        private readonly IIdentifierFactory idFactory;
        private readonly IRecorder recorder;
        private readonly IRepository repository;

        public ReadModelCheckpointStore(IRecorder recorder, IIdentifierFactory idFactory,
            IDomainFactory domainFactory, IRepository repository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            idFactory.GuardAgainstNull(nameof(idFactory));
            repository.GuardAgainstNull(nameof(repository));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            this.recorder = recorder;
            this.idFactory = idFactory;
            this.repository = repository;
            this.domainFactory = domainFactory;
        }

        private static string ContainerName => typeof(Checkpoint).GetEntityNameSafe();

        public long LoadCheckpoint(string streamName)
        {
            var checkpoint = GetCheckpoint(streamName);
            return checkpoint == null
                ? StartingCheckpointPosition
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
                checkpoint.Id = this.idFactory.Create(checkpoint);
                this.repository.Add(ContainerName, CommandEntity.FromType(checkpoint));
            }
            else
            {
                checkpoint.Position = position;
                this.repository.Replace(ContainerName, checkpoint.Id, CommandEntity.FromType(checkpoint));
            }

            this.recorder.TraceDebug("Saved checkpoint {StreamName} to position: {Position}", streamName,
                position);
        }

        private Checkpoint GetCheckpoint(string streamName)
        {
            var checkpoint = this.repository
                .Query(ContainerName,
                    Query.From<Checkpoint>().Where(cp => cp.StreamName, ConditionOperator.EqualTo, streamName),
                    RepositoryEntityMetadata.FromType<Checkpoint>())
                .FirstOrDefault();
            return checkpoint?.ToEntity<Checkpoint>(this.domainFactory);
        }
    }
}