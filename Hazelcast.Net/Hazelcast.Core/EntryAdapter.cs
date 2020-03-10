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

namespace Hazelcast.Core
{
    public class EntryAdapter<TKey, TValue> : IEntryListener<TKey, TValue>
    {
        public EntryAdapter()
        {
        }

        public EntryAdapter(Action<EntryEvent<TKey, TValue>> fAdded, Action<EntryEvent<TKey, TValue>> fRemoved,
            Action<EntryEvent<TKey, TValue>> fUpdated, Action<EntryEvent<TKey, TValue>> fEvicted)
        {
            Added = fAdded;
            Removed = fRemoved;
            Updated = fUpdated;
            Evicted = fEvicted;
        }

        public EntryAdapter(Action<EntryEvent<TKey, TValue>> fAdded, Action<EntryEvent<TKey, TValue>> fRemoved,
            Action<EntryEvent<TKey, TValue>> fUpdated, Action<EntryEvent<TKey, TValue>> fEvicted,
            Action<MapEvent> fEvictAll, Action<MapEvent> fClearAll)
        {
            Added = fAdded;
            Removed = fRemoved;
            Updated = fUpdated;
            Evicted = fEvicted;
            EvictAll = fEvictAll;
            ClearAll = fClearAll;
        }

        public Action<EntryEvent<TKey, TValue>> Added { get; set; }
        public Action<EntryEvent<TKey, TValue>> Evicted { get; set; }
        public Action<EntryEvent<TKey, TValue>> Removed { get; set; }
        public Action<EntryEvent<TKey, TValue>> Updated { get; set; }
        public Action<MapEvent> EvictAll { get; set; }
        public Action<MapEvent> ClearAll { get; set; }

        public void EntryAdded(EntryEvent<TKey, TValue> @event)
        {
            Added?.Invoke(@event);
        }

        public void EntryRemoved(EntryEvent<TKey, TValue> @event)
        {
            Removed?.Invoke(@event);
        }

        public void EntryUpdated(EntryEvent<TKey, TValue> @event)
        {
            Updated?.Invoke(@event);
        }

        public void EntryEvicted(EntryEvent<TKey, TValue> @event)
        {
            Evicted?.Invoke(@event);
        }

        public void MapEvicted(MapEvent @event)
        {
            EvictAll?.Invoke(@event);
        }

        public void MapCleared(MapEvent @event)
        {
            ClearAll?.Invoke(@event);
        }
    }
}