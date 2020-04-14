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
using System.Collections.Concurrent;
using Hazelcast.Core;

namespace Hazelcast.Eventing
{
    /// <summary>
    /// Implements <see cref="IEventHandlers{TEvent}"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of the events.</typeparam>
    internal class EventHandlers<TEvent> : IEventHandlers<TEvent>
    {
        private readonly ConcurrentDictionary<Guid, IEventHandler<TEvent>> _handlers
            = new ConcurrentDictionary<Guid, IEventHandler<TEvent>>();

        /// <inheritdoc />
        public Guid Add(IEventHandler<TEvent> handler)
        {
            var id = Guid.NewGuid();
            _handlers.AddOrUpdate(id, handler, (_, __) => handler);
            return id;
        }

        /// <inheritdoc />
        public bool Remove(Guid id)
        {
            return _handlers.TryRemove(id, out _);
        }

        /// <inheritdoc />
        public void Handle(TEvent eventData)
        {
            foreach (var (_, handler) in _handlers)
                handler.Handle(eventData);
        }
    }
}