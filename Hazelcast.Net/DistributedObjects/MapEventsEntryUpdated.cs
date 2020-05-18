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
    public sealed class MapEntryUpdatedEventArgs<TKey, TValue> : MapEntryEventArgsBase<TKey>
    {
        private readonly Lazy<TValue> _oldValue;
        private readonly Lazy<TValue> _value;

        public MapEntryUpdatedEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> oldValue, Lazy<TValue> value)
            : base(member, key)
        {
            _oldValue = oldValue;
            _value = value;
        }

        /// <summary>
        /// Gets the value before the update.
        /// </summary>
        public TValue OldValue => _oldValue == null ? default : _oldValue.Value;

        /// <summary>
        /// Gets the updated value.
        /// </summary>
        public TValue Value => _value == null ? default : _value.Value;
    }

    internal sealed class MapEntryUpdatedEventHandler<TKey, TValue> : MapEntryEventHandlerBase<TKey, TValue, MapEntryUpdatedEventArgs<TKey, TValue>>
    {
        public MapEntryUpdatedEventHandler(Action<IMap<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>> handler)
            : base(MapEventType.Updated, handler)
        { }

        protected override MapEntryUpdatedEventArgs<TKey, TValue> CreateEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventType eventType, int numberOfAffectedEntries)
            => new MapEntryUpdatedEventArgs<TKey, TValue>(member, key, oldValue, value);
    }

    public static partial class Extensions
    {
        public static MapEventHandlers<TKey, TValue> EntryUpdated<TKey, TValue>(this MapEventHandlers<TKey, TValue> handlers, Action<IMap<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>> handler)
        {
            handlers.Add(new MapEntryUpdatedEventHandler<TKey, TValue>(handler));
            return handlers;
        }
    }
}