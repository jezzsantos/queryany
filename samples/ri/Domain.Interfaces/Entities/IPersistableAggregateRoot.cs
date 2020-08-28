using System.Collections.Generic;

namespace Domain.Interfaces.Entities
{
    public interface IPersistableAggregateRoot : IPersistableEntity
    {
        int ChangeVersion { set; }

        List<EventEntity> GetChanges();

        void ClearChanges();

        void OnStateChanged(object @event);

        void LoadChanges(IEnumerable<EventEntity> history);
    }
}