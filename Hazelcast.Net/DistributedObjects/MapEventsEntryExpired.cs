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
using Hazelcast.Clustering;
using Hazelcast.Data;
using Hazelcast.Data.Map;

namespace Hazelcast.DistributedObjects
{
    public sealed class MapEntryExpiredEventArgs<TKey, TValue> : MapEntryEventArgsBase<TKey>
    {
        private readonly Lazy<TValue> _value;

        public MapEntryExpiredEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value)
            : base(member, key)
        {
            _value = value;
        }

        public TValue Value => _value == null ? default : _value.Value;
    }

    internal sealed class MapEntryExpiredEventHandler<TKey, TValue> : MapEntryEventHandlerBase<TKey, TValue, MapEntryExpiredEventArgs<TKey, TValue>>
    {
        public MapEntryExpiredEventHandler(Action<IMap<TKey, TValue>, MapEntryExpiredEventArgs<TKey, TValue>> handler)
            : base(MapEventType.Expired, handler)
        { }

        protected override MapEntryExpiredEventArgs<TKey, TValue> CreateEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergingValue, MapEventType eventType, int numberOfAffectedEntries)
            => new MapEntryExpiredEventArgs<TKey, TValue>(member, key, value);
    }

    public static partial class Extensions
    {
        public static MapEvents<TKey, TValue> EntryExpired<TKey, TValue>(this MapEvents<TKey, TValue> events, Action<IMap<TKey, TValue>, MapEntryExpiredEventArgs<TKey, TValue>> handler)
        {
            events.Handlers.Add(new MapEntryExpiredEventHandler<TKey, TValue>(handler));
            return events;
        }
    }
}