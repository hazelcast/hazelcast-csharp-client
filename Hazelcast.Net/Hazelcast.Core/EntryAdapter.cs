/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System;

namespace Hazelcast.Core
{
    public class EntryAdapter<K, V> : IEntryListener<K, V>
    {
        private readonly Action<EntryEvent<K, V>> fAdded;
        private readonly Action<EntryEvent<K, V>> fEvicted;
        private readonly Action<EntryEvent<K, V>> fRemoved;
        private readonly Action<EntryEvent<K, V>> fUpdated;
        private readonly Action<MapEvent> fEvictAll;
        private readonly Action<MapEvent> fClearAll;

        public EntryAdapter(Action<EntryEvent<K, V>> fAdded, Action<EntryEvent<K, V>> fRemoved,
            Action<EntryEvent<K, V>> fUpdated, Action<EntryEvent<K, V>> fEvicted)
        {
            this.fAdded = fAdded;
            this.fRemoved = fRemoved;
            this.fUpdated = fUpdated;
            this.fEvicted = fEvicted;
        }

        public EntryAdapter(Action<EntryEvent<K, V>> fAdded, Action<EntryEvent<K, V>> fRemoved,
            Action<EntryEvent<K, V>> fUpdated, Action<EntryEvent<K, V>> fEvicted, Action<MapEvent> fEvictAll, Action<MapEvent> fClearAll)
        {
            this.fAdded = fAdded;
            this.fRemoved = fRemoved;
            this.fUpdated = fUpdated;
            this.fEvicted = fEvicted;
            this.fEvictAll = fEvictAll;
            this.fClearAll = fClearAll;
        }


        public void EntryAdded(EntryEvent<K, V> @event)
        {
            fAdded(@event);
        }

        public void EntryRemoved(EntryEvent<K, V> @event)
        {
            fRemoved(@event);
        }

        public void EntryUpdated(EntryEvent<K, V> @event)
        {
            fUpdated(@event);
        }

        public void EntryEvicted(EntryEvent<K, V> @event)
        {
            fEvicted(@event);
        }

        public void MapEvicted(MapEvent @event)
        {
            fEvictAll(@event);
        }

        public void MapCleared(MapEvent @event)
        {
            fClearAll(@event);
        }
    }
}