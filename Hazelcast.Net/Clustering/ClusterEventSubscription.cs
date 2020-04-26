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
using System.Collections.Concurrent;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a cluster subscription to a server event.
    /// </summary>
    public class ClusterEventSubscription
    {
        private readonly Func<ClientMessage, object, Guid> _subscribeResponseParser;
        private readonly Func<Guid, object, ClientMessage> _unsubscribeRequestFactory;
        private readonly Action<ClientMessage, object> _eventHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterEventSubscription"/> class with an auto-assigned identifier.
        /// </summary>
        /// <param name="subscribeRequest">The subscribe request message.</param>
        /// <param name="subscribeResponseParser">The subscribe response message parser.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="state">A state object.</param>
        public ClusterEventSubscription(ClientMessage subscribeRequest, Func<ClientMessage, object, Guid> subscribeResponseParser, Action<ClientMessage, object> eventHandler, object state = null)
            : this(Guid.NewGuid(), subscribeRequest, subscribeResponseParser, null, eventHandler, state)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterEventSubscription"/> class with an auto-assigned identifier.
        /// </summary>
        /// <param name="subscribeRequest">The subscribe request message.</param>
        /// <param name="subscribeResponseParser">The subscribe response message parser.</param>
        /// <param name="unsubscribeRequestFactory">The unsubscribe request message factory.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="state">A state object.</param>
        public ClusterEventSubscription(ClientMessage subscribeRequest, Func<ClientMessage, object, Guid> subscribeResponseParser, Func<Guid, object, ClientMessage> unsubscribeRequestFactory, Action<ClientMessage, object> eventHandler, object state = null)
            : this(Guid.NewGuid(), subscribeRequest, subscribeResponseParser, unsubscribeRequestFactory, eventHandler, state)
        { }

        /// <summary>
        /// Initializes a new complete instance of the <see cref="ClusterEventSubscription"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <param name="subscribeRequest">The subscribe request message.</param>
        /// <param name="subscribeResponseParser">The subscribe response message parser.</param>
        /// <param name="unsubscribeRequestFactory">The unsubscribe request message factory.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="state">A state object.</param>
        public ClusterEventSubscription(Guid id, ClientMessage subscribeRequest, Func<ClientMessage, object, Guid> subscribeResponseParser, Func<Guid, object, ClientMessage> unsubscribeRequestFactory, Action<ClientMessage, object> eventHandler, object state = null)
        {
            Id = id;
            SubscribeRequest = subscribeRequest;
            _subscribeResponseParser = subscribeResponseParser;
            _unsubscribeRequestFactory = unsubscribeRequestFactory;
            _eventHandler = eventHandler;
            State = state;
        }

        /// <summary>
        /// Initializes a new simplified instance of the <see cref="ClusterEventSubscription"/> class.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <remarks>
        /// <para>A simplified instance only has a handler, and nothing else.</para>
        /// </remarks>
        internal ClusterEventSubscription(Action<ClientMessage, object> eventHandler)
        {
            _eventHandler = eventHandler;
        }

        /// <summary>
        /// Gets or sets the unique identifier of this subscription.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the state object.
        /// </summary>
        public object State { get; }

        /// <summary>
        /// Gets the registration request message.
        /// </summary>
        public ClientMessage SubscribeRequest { get; }

        /// <summary>
        /// Accepts a subscribe response message and returns the unique identifier assigned to the subscription by the server.
        /// </summary>
        /// <param name="message">The registration response message.</param>
        /// <param name="client">The client.</param>
        /// <returns>The client subscription.</returns>
        public ClientEventSubscription AcceptSubscribeResponse(ClientMessage message, Client client)
        {
            var serverSubscriptionId = _subscribeResponseParser(message, State);
            var clientSubscription = new ClientEventSubscription(this, serverSubscriptionId, SubscribeRequest.CorrelationId, client);
            ClientSubscriptions[clientSubscription.Client] = clientSubscription;
            return clientSubscription;
        }

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
        public void Handle(ClientMessage eventMessage) => _eventHandler(eventMessage, State);

        /// <summary>
        /// Gets the client subscriptions for this cluster subscription.
        /// </summary>
        public ConcurrentDictionary<Client, ClientEventSubscription> ClientSubscriptions { get; }
            = new ConcurrentDictionary<Client, ClientEventSubscription>();
    }
}