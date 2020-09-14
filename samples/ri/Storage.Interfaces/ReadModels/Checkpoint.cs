using System;
using Domain.Interfaces.Entities;
using QueryAny;

namespace Storage.Interfaces.ReadModels
{
    [EntityName("checkpoints")]
    public class Checkpoint : IIdentifiableEntity, IQueryableEntity
    {
        public string StreamName { get; set; }

        public long Position { get; set; }

        public DateTime? LastPersistedAtUtc { get; set; }

        public Identifier Id { get; set; }
    }
}