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
using Hazelcast.Data;
using Hazelcast.Data.Map;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Handles map events.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TArgs">The actual type of the arguments.</typeparam>
    internal abstract class MapEventHandlerBase<TKey, TValue, TArgs> : IMapEventHandler<TKey, TValue>
    {
        private readonly Action<IMap<TKey, TValue>, TArgs> _handler;

        protected MapEventHandlerBase(MapEventType eventType, Action<IMap<TKey, TValue>, TArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        public MapEventType EventType { get; }

        public void Handle(IMap<TKey, TValue> sender, MemberInfo member, int numberOfAffectedEntries)
            => _handler(sender, CreateEventArgs(member, numberOfAffectedEntries));

        protected abstract TArgs CreateEventArgs(MemberInfo member, int numberOfAffectedEntries);
    }
}