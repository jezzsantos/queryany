using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny;

namespace Storage.Interfaces
{
    [EntityName("checkpoints")]
    public class Checkpoint : IPersistableEntity
    {
        public string StreamName { get; set; }

        public long Position { get; set; }

        public Identifier Id { get; private set; }

        public DateTime? LastPersistedAtUtc { get; private set; }

        public Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id},
                {nameof(LastPersistedAtUtc), LastPersistedAtUtc},
                {nameof(StreamName), StreamName},
                {nameof(Position), Position}
            };
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            Id = properties.GetValueOrDefault<Identifier>(nameof(Id));
            LastPersistedAtUtc = properties.GetValueOrDefault<DateTime?>(nameof(LastPersistedAtUtc));
            StreamName = properties.GetValueOrDefault<string>(nameof(StreamName));
            Position = properties.GetValueOrDefault<long>(nameof(Position));
        }

        public void SetIdentifier(IIdentifierFactory idFactory)
        {
            Id = idFactory.Create(this);
        }
    }
}