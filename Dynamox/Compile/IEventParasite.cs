using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamox.Compile
{
    public delegate void EventShareHandler(EventShareEventArgs args);

    /// <summary>
    /// Hooks onto and raises the events of another object
    /// </summary>
    public interface IEventParasite
    {
        /// <summary>
        /// Signal to the host that it should raise an event
        /// </summary>
        event EventShareHandler RaiseEventCalled;

        /// <summary>
        /// Signals that an event occured on the host
        /// </summary>
        void EventRaised(EventShareEventArgs args);
    }

    public class EventShareEventArgs
    {
        public readonly string EventName;
        public readonly IEnumerable<object> EventArgs;

        bool _EventHandlerFound = false;
        public bool EventHandlerFound
        {
            get { return _EventHandlerFound; }
            set { _EventHandlerFound |= value; }
        }

        public EventShareEventArgs(string eventName, IEnumerable<object> eventArgs)
        {
            EventName = eventName;
            EventArgs = Array.AsReadOnly(eventArgs.ToArray());
        }
    }
}