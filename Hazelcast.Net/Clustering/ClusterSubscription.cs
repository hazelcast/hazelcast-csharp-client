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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a cluster subscription to a server event.
    /// </summary>
    public class ClusterSubscription : IEnumerable<ClientSubscription>
    {
        private readonly object _activeLock = new object();
        private readonly ConcurrentDictionary<Client, ClientSubscription> _clientSubscriptions = new ConcurrentDictionary<Client, ClientSubscription>();
        private readonly Func<ClientMessage, object, Guid> _subscribeResponseParser;
        private readonly Func<Guid, object, ClientMessage> _unsubscribeRequestFactory;
        private readonly Func<ClientMessage, object, bool> _unsubscribeResponseDecoder;
        private readonly Action<ClientMessage, object> _eventHandler;

        private volatile bool _active;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSubscription"/> class with an auto-assigned identifier.
        /// </summary>
        /// <param name="subscribeRequest">The subscribe request message.</param>
        /// <param name="subscribeResponseParser">The subscribe response message parser.</param>
        /// <param name="unsubscribeRequestFactory">The unsubscribe request message factory.</param>
        /// <param name="unsubscribeResponseDecoder">An unsubscribe response decoder.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="state">A state object.</param>
        public ClusterSubscription(ClientMessage subscribeRequest,
            Func<ClientMessage, object, Guid> subscribeResponseParser,
            Func<Guid, object, ClientMessage> unsubscribeRequestFactory,
            Func<ClientMessage, object, bool> unsubscribeResponseDecoder,
            Action<ClientMessage, object> eventHandler,
            object state = null)
            : this(Guid.NewGuid(), subscribeRequest, subscribeResponseParser, unsubscribeRequestFactory, unsubscribeResponseDecoder, eventHandler, state)
        { }

        /// <summary>
        /// Initializes a new complete instance of the <see cref="ClusterSubscription"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <param name="subscribeRequest">The subscribe request message.</param>
        /// <param name="subscribeResponseParser">The subscribe response message parser.</param>
        /// <param name="unsubscribeRequestFactory">The unsubscribe request message factory.</param>
        /// <param name="unsubscribeResponseDecoder">An unsubscribe response decoder.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="state">A state object.</param>
        public ClusterSubscription(Guid id, ClientMessage subscribeRequest,
            Func<ClientMessage, object, Guid> subscribeResponseParser,
            Func<Guid, object, ClientMessage> unsubscribeRequestFactory,
            Func<ClientMessage, object, bool> unsubscribeResponseDecoder,
            Action<ClientMessage, object> eventHandler,
            object state = null)
        {
            Id = id;
            SubscribeRequest = subscribeRequest;
            _subscribeResponseParser = subscribeResponseParser;
            _unsubscribeRequestFactory = unsubscribeRequestFactory;
            _unsubscribeResponseDecoder = unsubscribeResponseDecoder;
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
        internal ClusterSubscription(Action<ClientMessage, object> eventHandler)
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
            lock (_activeLock)
            {
                _active = false;
            }
        }

        /// <summary>
        /// Tries to add a client subscription.
        /// </summary>
        /// <param name="message">The subscription response message.</param>
        /// <param name="client">The client.</param>
        /// <returns>Whether the client subscription was added, and its server identifier.</returns>
        public (bool, Guid) TryAddClientSubscription(ClientMessage message, Client client)
        {
            var serverSubscriptionId = _subscribeResponseParser(message, State);

            bool active;
            lock (_activeLock)
            {
                active = _active;
                if (active)
                    _clientSubscriptions[client] = new ClientSubscription(this, serverSubscriptionId, SubscribeRequest.CorrelationId, client);
            }

            return (active, serverSubscriptionId);
        }

        /// <summary>
        /// Removes a client subscription.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="clientSubscription">The client subscription.</param>
        /// <returns>Whether a client subscription was removed.</returns>
        public bool TryRemove(Client client, out ClientSubscription clientSubscription)
            => _clientSubscriptions.TryRemove(client, out clientSubscription);

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
        public void Handle(ClientMessage eventMessage)
        {
            if (!_active)
                return;

            _eventHandler(eventMessage, State);
        }

        /// <summary>
        /// Decodes the unsubscribe response.
        /// </summary>
        /// <param name="message">The unsubscribe response.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool DecodeUnsubscribeResponse(ClientMessage message)
        {
            return _unsubscribeResponseDecoder(message, State);
        }

        /// <inheritdoc />
        public IEnumerator<ClientSubscription> GetEnumerator()
            => _clientSubscriptions.Values.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}