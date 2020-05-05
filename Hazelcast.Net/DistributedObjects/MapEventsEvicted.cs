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

namespace Hazelcast.DistributedObjects
{
    public sealed class MapEvictedEventArgs : MapEventArgsBase
    {
        public MapEvictedEventArgs(MemberInfo member, int numberOfAffectedEntries)
            : base(member, numberOfAffectedEntries)
        { }
    }

    internal sealed class MapEvictedEventHandler<TKey, TValue> : MapEventHandlerBase<TKey, TValue, MapEvictedEventArgs>
    {
        public MapEvictedEventHandler(Action<IMap<TKey, TValue>, MapEvictedEventArgs> handler)
            : base(MapEventType.AllEvicted, handler)
        { }

        protected override MapEvictedEventArgs CreateEventArgs(MemberInfo member, int numberOfAffectedEntries)
            => new MapEvictedEventArgs(member, numberOfAffectedEntries);
    }

    public static partial class Extensions
    {
        public static MapEvents<TKey, TValue> Evicted<TKey, TValue>(this MapEvents<TKey, TValue> events, Action<IMap<TKey, TValue>, MapEvictedEventArgs> handler)
        {
            events.Handlers.Add(new MapEvictedEventHandler<TKey, TValue>(handler));
            return events;
        }
    }
}