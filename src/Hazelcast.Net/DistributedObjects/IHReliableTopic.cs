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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects;

/// <summary>
/// The Reliable Topic has its own <see cref="IHRingBuffer{TItem}"/> to store events,
/// and has its own thread to each subscription to process messages.
/// </summary>
public interface IHReliableTopic<T> : IDistributedObject
{
    /// <summary>Subscribes to this reliable topic.</summary>
    /// <remarks>Each subscription has its own thread to process messages.</remarks>
    /// <param name="events">Set action to be executed on the received message.</param>
    /// <param name="handlerOptions">Options for <see cref="ReliableTopicEventHandlers{T}"/></param>
    /// <param name="state">A state object.</param>
    Task<Guid> SubscribeAsync(Action<ReliableTopicEventHandlers<T>> events, ReliableTopicEventHandlerOptions handlerOptions = default, object state = null);

    /// <summary>Stops receiving messages for the given message listener.</summary>
    /// <remarks>
    ///     Stops receiving messages for the given message listener. If the given listener already removed,
    ///     this method does nothing.
    /// </remarks>
    /// <param name="subscriptionId">Id of listener registration.</param>
    /// <returns>Whether the operation completed successfully.</returns>
    /// <remarks>
    /// <para>Once this method has been invoked, and whatever its result, the subscription is
    /// de-activated, and the thread will be released which means that no events will trigger anymore.</para>
    /// </remarks>
    ValueTask<bool> UnsubscribeAsync(Guid subscriptionId);

    /// <summary>
    /// Checks the subscription that exists and runs by given id.
    /// </summary>
    /// <param name="subscriptionId">Subscription id to check.</param>
    /// <returns>True, if subscription is existing and running; otherwise false.</returns>
    bool IsSubscription(Guid subscriptionId);

    /// <summary>Publishes the message to all subscribers of this topic.</summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task PublishAsync(T message, CancellationToken cancellationToken = default);
}
