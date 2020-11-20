using System;
using System.Collections.Generic;
using QueryAny;

namespace Domain.Interfaces.Entities
{
    public interface IPersistableAggregateRoot : IIdentifiableEntity, IQueryableEntity
    {
        DateTime? LastPersistedAtUtc { get; }

        long ChangeVersion { get; }

        List<EntityEvent> GetChanges();

        void ClearChanges();

        void OnStateChanged(IChangeEvent @event);

        void LoadChanges(IEnumerable<EntityEvent> history, IChangeEventMigrator migrator);
    }
}