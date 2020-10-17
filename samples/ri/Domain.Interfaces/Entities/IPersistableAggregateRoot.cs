using System;
using System.Collections.Generic;
using QueryAny;

namespace Domain.Interfaces.Entities
{
    public interface IPersistableAggregateRoot : IIdentifiableEntity, IQueryableEntity
    {
        DateTime? LastPersistedAtUtc { get; }
        
        int ChangeVersion { set; }

        List<EntityEvent> GetChanges();

        void ClearChanges();

        void OnStateChanged(IChangeEvent @event);

        void LoadChanges(IEnumerable<EntityEvent> history);
    }
}