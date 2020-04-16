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

namespace Hazelcast.Eventing
{
    /// <summary>
    /// Defines a collection of event handlers that can handle one type of events.
    /// </summary>
    /// <typeparam name="TEvent">The type of the events.</typeparam>
    internal interface IEventHandlers<TEvent>
    {
        /// <summary>
        /// Adds an event handler.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>The unique identifier that was assigned to the event handler.</returns>
        Guid Add(IEventHandler<TEvent> handler);

        /// <summary>
        /// Remove an event handler.
        /// </summary>
        /// <param name="id">The unique identifier of the event handler.</param>
        /// <returns>true if the event handler was removed; otherwise false.</returns>
        bool Remove(Guid id);

        /// <summary>
        /// Handles an event.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        void Handle(TEvent eventData);
    }

    // FIXME
    public interface IEventHandlers2<TEvent>
    {
        void Add(IEventHandler<TEvent> handler);
        void Remove(IEventHandler<TEvent> handler);
        void Clear();
        void Raise(TEvent eventData);
    }
}