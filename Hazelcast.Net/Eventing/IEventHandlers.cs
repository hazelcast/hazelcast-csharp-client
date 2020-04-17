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

// FIXME this is 'documentation'
// the one above mimics ListenerService.RegisterListener / .DeregisterListener
// these methods deal with the protocol side of events (now done in Cluster) + handling events
//
// what is the point of working with identifiers?
// - NearCache registers an invalidation event handler (never de-registers?)
// - PartitionService registers a partition lost event handler (can remove)
// - ProxyManager registers a distributed object event handler (can remove)
// - ClientProxy registers ... listeners (and can remove)
//
// the key is de-duplication - with delegates... how does it work?
// how can I add twice, and know which one to remove, if ListenerService is global?
//
// if I get 3 different map from a client... and register for a particular event 3 times,
// what happens when the event triggers? do I have to be ready to be triggered ?!?!
//
// how will events be segregated by map? = they are, via CorrelationId
//
// now should we use the same system everywhere, or what???
// should we support async events, too?

namespace Hazelcast.Eventing
{
    /// <summary>
    /// Defines a collection of event handlers that can handle one type of events.
    /// </summary>
    /// <typeparam name="TEvent">The type of the events.</typeparam>
    public interface IEventHandlers<TEvent>
    {
        /// <summary>
        /// Adds an event handler.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        void Add(IEventHandler<TEvent> handler);

        /// <summary>
        /// Remove an event handler.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>true if the event handler was removed; otherwise false.</returns>
        bool Remove(IEventHandler<TEvent> handler);

        /// <summary>
        /// Removes all event handlers.
        /// </summary>
        void Clear();

        /// <summary>
        /// Handles an event.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        void Handle(TEvent eventData);
    }
}