using System.Collections.Generic;

namespace Domain.Interfaces.Entities
{
    public interface IPublishingEntity
    {
        void RaiseEvent(object @event);
    }

    public interface IPublishingAggregateEntity : IPublishingEntity
    {
        List<object> Events { get; }

        void ClearEvents();
    }
}