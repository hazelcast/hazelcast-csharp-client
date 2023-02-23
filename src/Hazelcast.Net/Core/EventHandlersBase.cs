// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

        // note: adding and removing handlers is not thread-safe
        // - adding happens when subscribing, which is not multi-threaded
        // - removing happens when unsubscribing, and a subscription should be unsubscribed
        //   only once, and once at a time, so it should not require thread-safety

        /// <summary>
        /// Adds a handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        protected void Add(TEventHandler handler)
            => _handlers.Add(handler);

        /// <inheritdoc />
        public IEnumerator<TEventHandler> GetEnumerator()
            => _handlers.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Removes a handler.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        public void Remove(TEventHandler handler)
            => _handlers.Remove(handler);
    }
}
