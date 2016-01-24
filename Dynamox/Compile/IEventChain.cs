using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamox.Compile
{
    public delegate void EventChainHandler(EventChainArgs args);

    /// <summary>
    /// Signals that the implementing class has events to raise
    /// </summary>
    public interface IEventChain
    {
        /// <summary>
        ///  Tell a parent object that an event occured on this object
        /// </summary>
        event EventChainHandler EventBubble;

        /// <summary>
        /// Tell this object that an event happened on a parent object
        /// </summary>
        void EventTunnel(EventChainArgs eventArgs);
    }

    public interface IEventChainArgs
    {
        string EventName { get; }
        IEnumerable<object> EventArgs { get; }
        IEnumerable<object> SenderChain { get; }
        bool EventHandlerFound { get; set; }
    }

    public class EventChainArgs : IEventChainArgs
    {
        public readonly IEventChainArgs Root;
        public readonly object[] SenderAsArray;

        public string EventName
        {
            get { return Root.EventName; }
        }

        public IEnumerable<object> EventArgs
        {
            get { return Root.EventArgs; }
        }

        public IEnumerable<object> SenderChain
        {
            get { return Root.SenderChain.Concat(SenderAsArray); }
        }

        //TODO: rename and protect against being set back to false
        public bool EventHandlerFound
        {
            get { return Root.EventHandlerFound; }
            set { Root.EventHandlerFound |= value; }
        }

        public EventChainArgs(object sender, string eventName, IEnumerable<object> eventArgs)
            : this(sender, new RootEventChainArgs(eventName, eventArgs))
        {
        }

        public EventChainArgs(object sender, IEventChainArgs existingChain)
        {
            if (existingChain.SenderChain.Contains(sender))
                throw new InvalidOperationException();  //TODE: circular reference

            Root = existingChain;
            SenderAsArray = new[] { sender };
        }

        public bool HasBeenRaisedBy(object sender) 
        {
            return SenderChain.Contains(sender);
        }

        class RootEventChainArgs : IEventChainArgs
        {
            public bool EventHandlerFound { get; set; }
            public string EventName { get; private set; }
            public IEnumerable<object> EventArgs { get; private set; }
            public IEnumerable<object> SenderChain
            {
                get { return Enumerable.Empty<object>(); }
            }

            public RootEventChainArgs(string eventName, IEnumerable<object> eventArgs)
            {
                EventName = eventName;
                EventArgs = Array.AsReadOnly(eventArgs.ToArray());
                EventHandlerFound = false;
            }
        }
    }
}