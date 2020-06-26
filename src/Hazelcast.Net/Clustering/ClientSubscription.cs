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

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a client subscription to a server event.
    /// </summary>
    internal class ClientSubscription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSubscription"/> class.
        /// </summary>
        /// <param name="clusterSubscription">The cluster subscription.</param>
        /// <param name="serverSubscriptionId">The unique identifier assigned by the server to the client subscription.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="client">The subscribed client.</param>
        public ClientSubscription(ClusterSubscription clusterSubscription, Guid serverSubscriptionId, long correlationId, ClientConnection client)
        {
            ClusterSubscription = clusterSubscription;
            ServerSubscriptionId = serverSubscriptionId;
            CorrelationId = correlationId;
            Client = client;
        }

        /// <summary>
        /// Gets the cluster subscription owning this client subscription.
        /// </summary>
        public ClusterSubscription ClusterSubscription { get; }

        /// <summary>
        /// Gets the unique identifier assigned by the server to this subscription.
        /// </summary>
        /// <remarks>
        /// <para>This is the identifier that needs to be used to unsubscribe.</para>
        /// </remarks>
        public Guid ServerSubscriptionId { get; }

        /// <summary>
        /// Gets the correlation identifier.
        /// </summary>
        /// <remarks>
        /// <para>Event messages for this subscription will have this correlation identifier.</para>
        /// </remarks>
        public long CorrelationId { get; }

        /// <summary>
        /// Gets the client.
        /// </summary>
        public ClientConnection Client { get; }
    }
}
