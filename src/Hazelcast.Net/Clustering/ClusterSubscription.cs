// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a cluster subscription to a server event.
    /// </summary>
    internal class ClusterSubscription : IEnumerable<MemberSubscription>
    {
        private readonly object _mutex = new object();
        private readonly ConcurrentDictionary<MemberConnection, MemberSubscription> _memberSubscriptions = new ConcurrentDictionary<MemberConnection, MemberSubscription>();
        private readonly Func<ClientMessage, object, Guid> _subscribeResponseReader;
        private readonly Func<Guid, object, ClientMessage> _unsubscribeRequestFactory;
        private readonly Func<ClientMessage, object, bool> _unsubscribeResponseReader;
        private readonly Func<ClientMessage, object, ValueTask> _eventHandler;

        private volatile bool _active = true; // always start as active

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSubscription"/> class with an auto-assigned identifier.
        /// </summary>
        /// <param name="subscribeRequest">The subscribe request message.</param>
        /// <param name="subscribeResponseReader">The subscribe response message reader.</param>
        /// <param name="unsubscribeRequestFactory">The unsubscribe request message factory.</param>
        /// <param name="unsubscribeResponseReader">An unsubscribe response reader.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="state">A state object.</param>
        public ClusterSubscription(ClientMessage subscribeRequest,
            Func<ClientMessage, object, Guid> subscribeResponseReader,
            Func<Guid, object, ClientMessage> unsubscribeRequestFactory,
            Func<ClientMessage, object, bool> unsubscribeResponseReader,
            Func<ClientMessage, object, ValueTask> eventHandler,
            object state = null)
            : this(Guid.NewGuid(), subscribeRequest, subscribeResponseReader, unsubscribeRequestFactory, unsubscribeResponseReader, eventHandler, state)
        { }

        /// <summary>
        /// Initializes a new complete instance of the <see cref="ClusterSubscription"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <param name="subscribeRequest">The subscribe request message.</param>
        /// <param name="subscribeResponseReader">The subscribe response message reader.</param>
        /// <param name="unsubscribeRequestFactory">The unsubscribe request message factory.</param>
        /// <param name="unsubscribeResponseReader">An unsubscribe response reader.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="state">A state object.</param>
        public ClusterSubscription(Guid id, ClientMessage subscribeRequest,
            Func<ClientMessage, object, Guid> subscribeResponseReader,
            Func<Guid, object, ClientMessage> unsubscribeRequestFactory,
            Func<ClientMessage, object, bool> unsubscribeResponseReader,
            Func<ClientMessage, object, ValueTask> eventHandler,
            object state = null)
        {
            Id = id;
            SubscribeRequest = subscribeRequest;
            _subscribeResponseReader = subscribeResponseReader;
            _unsubscribeRequestFactory = unsubscribeRequestFactory;
            _unsubscribeResponseReader = unsubscribeResponseReader;
            _eventHandler = eventHandler;
            State = state;
        }

        /// <summary>
        /// Initializes a new simplified instance of the <see cref="ClusterSubscription"/> class.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <remarks>
        /// <para>A simplified instance only has a handler, and nothing else. It cannot be unsubscribed,
        /// because it is bound to a client and just dies when the client dies. It will not have any
        /// associated client subscriptions.</para>
        /// </remarks>
        internal ClusterSubscription(Func<ClientMessage, object, ValueTask> eventHandler)
        {
            _eventHandler = eventHandler;
        }

        /// <summary>
        /// Gets or sets the unique identifier of this subscription.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets a value indicating whether the subscription is active.
        /// </summary>
        public bool Active => _active;

        /// <summary>
        /// Gets the state object.
        /// </summary>
        public object State { get; }

        /// <summary>
        /// Gets the registration request message.
        /// </summary>
        public ClientMessage SubscribeRequest { get; }

        /// <summary>
        /// Deactivates the subscription.
        /// </summary>
        public void Deactivate()
        {
            lock (_mutex)
            {
                _active = false;
                DeactivateTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Gets the time the subscription was de-activated.
        /// </summary>
        public DateTime DeactivateTime { get; private set; }

        /// <summary>
        /// Reads a subscription response.
        /// </summary>
        /// <param name="message">The response message.</param>
        /// <param name="connection">The connection.</param>
        /// <returns>The corresponding member subscription.</returns>
        public MemberSubscription ReadSubscriptionResponse(ClientMessage message, MemberConnection connection)
        {
            var serverSubscriptionId = _subscribeResponseReader(message, State);
            return new MemberSubscription(this, serverSubscriptionId, message.CorrelationId, connection);
        }

        /// <summary>
        /// Tries to add a member subscription.
        /// </summary>
        /// <param name="subscription">The member subscription.</param>
        /// <returns>Whether the member subscription was added.</returns>
        public bool TryAddMemberSubscription(MemberSubscription subscription)
        {
            lock (_mutex)
            {
                if (_active)
                    _memberSubscriptions[subscription.Connection] = subscription;
                return _active;
            }
        }

        /// <summary>
        /// Removes a member subscription.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="memberSubscription">The client subscription.</param>
        /// <returns>Whether the member subscription was removed.</returns>
        public bool TryRemove(MemberConnection connection, out MemberSubscription memberSubscription)
            => _memberSubscriptions.TryRemove(connection, out memberSubscription);

        /// <summary>
        /// Removes a member subscription.
        /// </summary>
        /// <param name="memberSubscription">The client subscription.</param>
        public void Remove(MemberSubscription memberSubscription)
            => _memberSubscriptions.TryRemove(memberSubscription.Connection, out _);

        /// <summary>
        /// Creates an unsubscribe request message.
        /// </summary>
        /// <param name="serverSubscriptionId">The unique identifier assigned to the subscription by the server.</param>
        /// <returns>A new unsubscribe request message.</returns>
        public ClientMessage CreateUnsubscribeRequest(Guid serverSubscriptionId) => _unsubscribeRequestFactory(serverSubscriptionId, State);

        /// <summary>
        /// Handles an event message.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public async ValueTask HandleAsync(ClientMessage eventMessage)
        {
            if (!_active) return;
            await _eventHandler(eventMessage, State).CfAwait();
        }

        /// <summary>
        /// Reads the unsubscribe response.
        /// </summary>
        /// <param name="message">The unsubscribe response.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool ReadUnsubscribeResponse(ClientMessage message)
            => _unsubscribeResponseReader(message, State);

        /// <summary>
        /// Gets the number of member subscriptions.
        /// </summary>
        public int Count => _memberSubscriptions.Count;

        /// <inheritdoc />
        public IEnumerator<MemberSubscription> GetEnumerator()
            // .Values captures values and can then be safely enumerated
            => _memberSubscriptions.Values.GetEnumerator(); 

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
