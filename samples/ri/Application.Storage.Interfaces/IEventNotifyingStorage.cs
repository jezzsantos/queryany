﻿using System.Collections.Generic;
using Common;

namespace Application.Storage.Interfaces
{
    public interface IEventNotifyingStorage
    {
        event EventStreamStateChanged OnEventStreamStateChanged;
    }

    public delegate void EventStreamStateChanged(object sender, EventStreamStateChangedArgs args);

    public class EventStreamStateChangedArgs
    {
        public EventStreamStateChangedArgs(List<EventStreamStateChangeEvent> events)
        {
            events.GuardAgainstNull(nameof(events));
            Events = events;
        }

        public List<EventStreamStateChangeEvent> Events { get; }
    }
}