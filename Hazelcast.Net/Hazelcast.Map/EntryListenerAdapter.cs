// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Map
{
    internal class EntryListenerAdapter<TKey, TValue> : EntryAddedListener<TKey, TValue>, EntryUpdatedListener<TKey, TValue>,
        EntryRemovedListener<TKey, TValue>, EntryEvictedListener<TKey, TValue>, EntryLoadedListener<TKey, TValue>,
        EntryMergedListener<TKey, TValue>, EntryExpiredListener<TKey, TValue>, MapClearedListener, MapEvictedListener
    {
        public ISerializationService SerializationService { get; set; }
        public EntryEventType ListenerFlags { get; set; }

        public Action<EntryEvent<TKey, TValue>> Added { get; set; }
        public Action<EntryEvent<TKey, TValue>> Evicted { get; set; }
        public Action<EntryEvent<TKey, TValue>> Removed { get; set; }
        public Action<EntryEvent<TKey, TValue>> Updated { get; set; }
        public Action<EntryEvent<TKey, TValue>> Merged { get; set; }
        public Action<EntryEvent<TKey, TValue>> Expired { get; set; }
        public Action<EntryEvent<TKey, TValue>> Loaded { get; set; }
        public Action<MapEvent> EvictAll { get; set; }
        public Action<MapEvent> ClearAll { get; set; }


        public void EntryAdded(EntryEvent<TKey, TValue> entryEvent)
        {
            Added?.Invoke(entryEvent);
        }

        public void EntryRemoved(EntryEvent<TKey, TValue> entryEvent)
        {
            Removed?.Invoke(entryEvent);
        }

        public void EntryUpdated(EntryEvent<TKey, TValue> entryEvent)
        {
            Updated?.Invoke(entryEvent);
        }

        public void EntryEvicted(EntryEvent<TKey, TValue> entryEvent)
        {
            Evicted?.Invoke(entryEvent);
        }

        public void MapEvicted(MapEvent mapEvent)
        {
            EvictAll?.Invoke(mapEvent);
        }

        public void MapCleared(MapEvent mapEvent)
        {
            ClearAll?.Invoke(mapEvent);
        }

        public void EntryMerged(EntryEvent<TKey, TValue> entryEvent)
        {
            Merged?.Invoke(entryEvent);
        }

        public void EntryExpired(EntryEvent<TKey, TValue> entryEvent)
        {
            Expired?.Invoke(entryEvent);
        }

        public void EntryLoaded(EntryEvent<TKey, TValue> entryEvent)
        {
            Loaded?.Invoke(entryEvent);
        }

        public void OnEntryEvent(string source, IData keyData, IData valueData, IData oldValueData, IData mergingValue,
            EntryEventType eventType, IMember member, int numberOfAffectedEntries)
        {
            if (eventType.HasFlag(EntryEventType.EvictAll) || eventType.HasFlag(EntryEventType.ClearAll))
            {
                var mapEvent = new MapEvent(source, member, eventType, numberOfAffectedEntries);
                switch (eventType)
                {
                    case EntryEventType.EvictAll:
                    {
                        MapEvicted(mapEvent);
                        break;
                    }
                    case EntryEventType.ClearAll:
                    {
                        MapCleared(mapEvent);
                        break;
                    }
                }
            }
            else
            {
                var dataAwareEvent = new DataAwareEntryEvent<TKey, TValue>(source, member, eventType, keyData,
                    valueData, oldValueData, mergingValue, SerializationService);
                switch (eventType)
                {
                    case EntryEventType.Added:
                    {
                        EntryAdded(dataAwareEvent);
                        break;
                    }
                    case EntryEventType.Removed:
                    {
                        EntryRemoved(dataAwareEvent);
                        break;
                    }
                    case EntryEventType.Updated:
                    {
                        EntryUpdated(dataAwareEvent);
                        break;
                    }
                    case EntryEventType.Evicted:
                    {
                        EntryEvicted(dataAwareEvent);
                        break;
                    }
                    case EntryEventType.Expired:
                    {
                        EntryExpired(dataAwareEvent);
                        break;
                    }
                    case EntryEventType.Merged:
                    {
                        EntryMerged(dataAwareEvent);
                        break;
                    }
                    case EntryEventType.Loaded:
                    {
                        EntryLoaded(dataAwareEvent);
                        break;
                    }
                }
            }
        }

        public static EntryListenerAdapter<TKey, TValue> CreateAdapter(MapListener mapListener,
            ISerializationService serializationService)
        {
            var adapter = new EntryListenerAdapter<TKey, TValue>
            {
                SerializationService = serializationService
            };
            if (mapListener is EntryAddedListener<TKey, TValue>)
            {
                adapter.ListenerFlags |= EntryEventType.Added;
                adapter.Added = ((EntryAddedListener<TKey, TValue>) mapListener).EntryAdded;
            }
            if (mapListener is EntryRemovedListener<TKey, TValue>)
            {
                adapter.ListenerFlags |= EntryEventType.Removed;
                adapter.Removed = ((EntryRemovedListener<TKey, TValue>) mapListener).EntryRemoved;
            }
            if (mapListener is EntryUpdatedListener<TKey, TValue>)
            {
                adapter.ListenerFlags |= EntryEventType.Updated;
                adapter.Updated = ((EntryUpdatedListener<TKey, TValue>) mapListener).EntryUpdated;
            }
            if (mapListener is EntryEvictedListener<TKey, TValue>)
            {
                adapter.ListenerFlags |= EntryEventType.Evicted;
                adapter.Evicted = ((EntryEvictedListener<TKey, TValue>) mapListener).EntryEvicted;
            }
            if (mapListener is EntryLoadedListener<TKey, TValue>)
            {
                adapter.ListenerFlags |= EntryEventType.Loaded;
                adapter.Loaded = ((EntryLoadedListener<TKey, TValue>) mapListener).EntryLoaded;
            }
            if (mapListener is MapEvictedListener)
            {
                adapter.ListenerFlags |= EntryEventType.EvictAll;
                adapter.EvictAll = ((MapEvictedListener) mapListener).MapEvicted;
            }
            if (mapListener is MapClearedListener)
            {
                adapter.ListenerFlags |= EntryEventType.ClearAll;
                adapter.ClearAll = ((MapClearedListener) mapListener).MapCleared;
            }
            if (mapListener is EntryMergedListener<TKey, TValue>)
            {
                adapter.ListenerFlags |= EntryEventType.Merged;
                adapter.Merged = ((EntryMergedListener<TKey, TValue>) mapListener).EntryMerged;
            }
            if (mapListener is EntryExpiredListener<TKey, TValue>)
            {
                adapter.ListenerFlags |= EntryEventType.Expired;
                adapter.Expired = ((EntryExpiredListener<TKey, TValue>) mapListener).EntryExpired;
            }
            return adapter;
        }
    }
}