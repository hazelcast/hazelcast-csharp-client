// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using Hazelcast.Logging;

// FIXME document and explain
// - why do we need Guids to identify handlers?
// - why do we need IEventHandler<TEvent> and not simply Action<TEvent> (or Func<TEvent, Task>)?
// - why have EventHandlers *and* EventHandlers2 and not do everything the same?
//
// traditional events work with just a delegate
//
// when an event message is received, it has a correlation id
// and the cluster has 'event handlers' which are Action<ClientMessage> one per correlation id
//
// the cluster keeps track of 'subscriptions'
//   so it has everything required for unsubscribing
//   should also keep the immutable correlationId?
// and registers the associated handler with the correlation id
//
//

namespace Hazelcast.Eventing
{
    /// <summary>
    /// Represents a collection of event handlers that can handle one type of events.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    internal class EventHandlers<TEvent> : IEventHandlers<TEvent>
    {
        // implementation notes
        // this is using a locked list - simple enough and fast - other concurrent structures
        // are way heavier, and also take shallow snapshots before enumerating - so before
        // replacing this with more a complex solution, benchmark to be sure it's actually
        // faster - also, using a list sort-of ensures a deterministic order of events, and
        // even if this should not be relied upon, it may make things cleaner

        private readonly List<IEventHandler<TEvent>> _handlers = new List<IEventHandler<TEvent>>();

        /// <inheritdoc />
        public void Add(IEventHandler<TEvent> handler)
        {
            lock (_handlers) _handlers.Add(handler);
        }

        /// <inheritdoc />
        public bool Remove(IEventHandler<TEvent> handler)
        {
            lock (_handlers) return _handlers.Remove(handler);
        }

        /// <inheritdoc />
        public void Clear()
        {
            lock (_handlers) _handlers.Clear();
        }

        /// <inheritdoc />
        public void Handle(TEvent eventData)
        {
            List<IEventHandler<TEvent>> snapshot;
            lock (_handlers) snapshot = new List<IEventHandler<TEvent>>(_handlers);

            foreach (var handler in snapshot)
            {
                try
                {
                    handler.Handle(eventData);
                }
                catch (Exception e)
                {
                    // we cannot let one handler kill everything,
                    // so are we going to swallow the exception?
                    XConsole.WriteLine(this, e);
                }
            }
        }
    }
}