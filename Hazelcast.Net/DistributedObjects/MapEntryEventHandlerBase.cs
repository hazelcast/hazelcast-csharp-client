﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.DistributedObjects
{
    internal abstract class MapEntryEventHandlerBase<TKey, TValue, TArgs> : IMapEntryEventHandler<TKey, TValue>
    {
        private readonly Action<IMap<TKey, TValue>, TArgs> _handler;

        protected MapEntryEventHandlerBase(EntryEventType eventType, Action<IMap<TKey, TValue>, TArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        public EntryEventType EventType { get; }

        public void Handle(IMap<TKey, TValue> sender, MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergingValue, EntryEventType eventType, int numberOfAffectedEntries)
            => _handler(sender, CreateEventArgs(member, key, value, oldValue, mergingValue, eventType, numberOfAffectedEntries));

        protected abstract TArgs CreateEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergingValue, EntryEventType eventType, int numberOfAffectedEntries);
    }
}