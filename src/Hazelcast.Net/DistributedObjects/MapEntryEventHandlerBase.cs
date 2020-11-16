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
using System.Threading.Tasks;
using Hazelcast.Data;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Handles map entry events.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    /// <typeparam name="TArgs">The actual type of the arguments.</typeparam>
    internal abstract class MapEntryEventHandlerBase<TKey, TValue, TSender, TArgs> : IMapEntryEventHandler<TKey, TValue, TSender>
    {
        private readonly Func<TSender, TArgs, ValueTask> _handler;

        protected MapEntryEventHandlerBase(MapEventTypes eventType, Func<TSender, TArgs, ValueTask> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        public MapEventTypes EventType { get; }

        public ValueTask HandleAsync(TSender sender, MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventTypes eventType, int numberOfAffectedEntries, object state)
            => _handler(sender, CreateEventArgs(member, key, value, oldValue, mergeValue, eventType, numberOfAffectedEntries, state));

        protected abstract TArgs CreateEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventTypes eventType, int numberOfAffectedEntries, object state);
    }
}
