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
using Hazelcast.Predicates;

namespace Hazelcast.DistributedObjects
{
    public partial interface IHReplicatedMap<TKey, TValue> // Events
    {
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="handle">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> handle);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="key">A key to filter events.</param>
        /// <param name="handle">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(TKey key, Action<ReplicatedMapEventHandlers<TKey, TValue>> handle);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="handle">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(IPredicate predicate, Action<ReplicatedMapEventHandlers<TKey, TValue>> handle);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="key">A key to filter events.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="handle">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<ReplicatedMapEventHandlers<TKey, TValue>> handle);

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>Whether the operation was successful.</returns>
        /// <remarks>
        /// <para>Once this method has been invoked, and whatever its result, the subscription is
        /// de-activated, which means that no events will trigger anymore, even if the client
        /// receives event messages from the servers.</para>
        /// <para>If this method returns <c>false</c>, then one or more client connection has not
        /// been able to get its server to remove the subscription. Even though no events will
        /// trigger anymore, the server may keep sending (ignored) event messages. It is therefore
        /// recommended to retry unsubscribing until it is successful.</para>
        /// </remarks>
        ValueTask<bool> UnsubscribeAsync(Guid subscriptionId);
    }
}
