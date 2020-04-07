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

namespace Hazelcast.Eventing
{
    internal class EventHandlers<TEvent> : IEventHandlers<TEvent>
    {
        private readonly ConcurrentDictionary<Guid, IEventHandler<TEvent>> _listeners
            = new ConcurrentDictionary<Guid, IEventHandler<TEvent>>();

        public Guid Add(IEventHandler<TEvent> listener)
        {
            var id = Guid.NewGuid();
            _listeners.AddOrUpdate(id, id => listener, (id, _) => listener);
            return id;
        }


        public bool Remove(Guid id)
        {
            return _listeners.TryRemove(id, out _);
        }

        public void Handle(TEvent e)
        {
            foreach (var (_, listener) in _listeners)
                listener.Handle(e);
        }
    }
}