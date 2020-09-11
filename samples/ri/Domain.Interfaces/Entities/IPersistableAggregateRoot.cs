using System.Collections.Generic;

namespace Domain.Interfaces.Entities
{
    public interface IPersistableAggregateRoot : IPersistableEntity
    {
        int ChangeVersion { set; }

        List<EntityEvent> GetChanges();

        void ClearChanges();

        void OnStateChanged(object @event);

        void LoadChanges(IEnumerable<EntityEvent> history);
    }
}