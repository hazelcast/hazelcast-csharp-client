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
using System.Collections.Generic;
using Hazelcast.Clustering;
using Hazelcast.Data;
using Hazelcast.Data.Map;

namespace Hazelcast.DistributedObjects
{
    // TODO: see TopicEvents, all these classes must be documented

    public class MapEvents<TKey, TValue>
    {
        internal List<IMapEventHandlerBase<TKey, TValue>> Handlers { get; } = new List<IMapEventHandlerBase<TKey, TValue>>();
    }

    public interface IMapEventHandlerBase<TKey, TValue> // FIXME: validate generic type?
    {
        MapEventType EventType { get; }
    }

    public interface IMapEventHandler<TKey, TValue> : IMapEventHandlerBase<TKey, TValue>
    {
        void Handle(IMap<TKey, TValue> sender, MemberInfo member, int numberOfAffectedEntries);
    }

    public interface IMapEntryEventHandler<TKey, TValue> : IMapEventHandlerBase<TKey, TValue>
    {
        void Handle(IMap<TKey, TValue> sender, MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventType eventType, int numberOfAffectedEntries);
    }

    public abstract class MapEventArgsBase
    {
        protected MapEventArgsBase(MemberInfo member, int numberOfAffectedEntries)
        {
            Member = member;
            NumberOfAffectedEntries = numberOfAffectedEntries;
        }


        public MemberInfo Member { get; }

        public int NumberOfAffectedEntries { get; }
    }

    public abstract class MapEntryEventArgsBase<TKey>
    {
        private readonly Lazy<TKey> _key;

        protected MapEntryEventArgsBase(MemberInfo member, Lazy<TKey> key)
        {
            Member = member;
            _key = key;
        }

        public MemberInfo Member { get; }

        public TKey Key => _key == null ? default : _key.Value;
    }

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

    internal abstract class MapEntryEventHandlerBase<TKey, TValue, TArgs> : IMapEntryEventHandler<TKey, TValue>
    {
        private readonly Action<IMap<TKey, TValue>, TArgs> _handler;

        protected MapEntryEventHandlerBase(MapEventType eventType, Action<IMap<TKey, TValue>, TArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        public MapEventType EventType { get; }

        public void Handle(IMap<TKey, TValue> sender, MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventType eventType, int numberOfAffectedEntries)
            => _handler(sender, CreateEventArgs(member, key, value, oldValue, mergeValue, eventType, numberOfAffectedEntries));

        protected abstract TArgs CreateEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, MapEventType eventType, int numberOfAffectedEntries);
    }
}