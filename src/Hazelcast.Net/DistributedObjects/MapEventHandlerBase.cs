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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Data;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Handles map events.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TSender">The type of the sender.</typeparam>
    /// <typeparam name="TArgs">The actual type of the arguments.</typeparam>
    internal abstract class MapEventHandlerBase<TKey, TValue, TSender, TArgs> : IMapEventHandler<TKey, TValue, TSender>
    {
        private readonly Func<TSender, TArgs, CancellationToken, ValueTask> _handler;

        protected MapEventHandlerBase(MapEventTypes eventType, Func<TSender, TArgs, CancellationToken, ValueTask> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        public MapEventTypes EventType { get; }

        public ValueTask HandleAsync(TSender sender, MemberInfo member, int numberOfAffectedEntries, CancellationToken cancellationToken)
            => _handler(sender, CreateEventArgs(member, numberOfAffectedEntries), cancellationToken);

        protected abstract TArgs CreateEventArgs(MemberInfo member, int numberOfAffectedEntries);
    }
}
