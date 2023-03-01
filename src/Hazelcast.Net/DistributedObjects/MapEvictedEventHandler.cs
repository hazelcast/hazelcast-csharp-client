// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    internal sealed class MapEvictedEventHandler<TKey, TValue, TSender> : MapEventHandlerBase<TKey, TValue, TSender, MapEvictedEventArgs>
    {
        public MapEvictedEventHandler(Func<TSender, MapEvictedEventArgs, ValueTask> handler)
            : base(MapEventTypes.AllEvicted, handler)
        { }

        protected override MapEvictedEventArgs CreateEventArgs(MemberInfo member, int numberOfAffectedEntries, object state)
            => new MapEvictedEventArgs(member, numberOfAffectedEntries, state);
    }
}
