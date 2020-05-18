using System.Collections;
using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides a base class for classes containing event handlers.
    /// </summary>
    /// <typeparam name="TEventHandler">The type of the event handlers.</typeparam>
    public abstract class EventHandlersBase<TEventHandler> : IEnumerable<TEventHandler>
    {
        private readonly List<TEventHandler> _handlers = new List<TEventHandler>();

        /// <summary>
        /// Adds a handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void Add(TEventHandler handler)
            => _handlers.Add(handler);

        /// <inheritdoc />
        public IEnumerator<TEventHandler> GetEnumerator()
            => _handlers.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
