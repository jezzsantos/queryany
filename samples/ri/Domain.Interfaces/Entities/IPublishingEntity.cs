using System.Collections.Generic;

namespace Domain.Interfaces.Entities
{
    public interface IPublishingEntity
    {
        List<object> Events { get; }

        void ClearEvents();

        void RaiseEvent(object @event);
    }
}