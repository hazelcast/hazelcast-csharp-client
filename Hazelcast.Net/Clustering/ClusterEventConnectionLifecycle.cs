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
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Defines the types of connection lifecycle events.
    /// </summary>
    internal enum ConnectionLifecycleEventType
    {
        /// <summary>
        /// Nothing (default).
        /// </summary>
        Nothing = 0,

        /// <summary>
        /// A connection was added.
        /// </summary>
        Added,

        /// <summary>
        /// A connection was removed.
        /// </summary>
        Removed
    }

    /// <summary>
    /// Represents event data for connection lifecycle events.
    /// </summary>
    public class ConnectionLifecycleEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionLifecycleEventArgs"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public ConnectionLifecycleEventArgs(Client client)
        {
            Client = client;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        public Client Client { get; }
    }

    internal class ConnectionLifecycleEventHandler : IClusterEventHandler
    {
        private readonly Action<Cluster, ConnectionLifecycleEventArgs> _handler;

        public ConnectionLifecycleEventHandler(ConnectionLifecycleEventType eventType, Action<Cluster, ConnectionLifecycleEventArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        public ConnectionLifecycleEventType EventType { get; }

        public void Handle(Cluster cluster, Client client)
            => _handler(cluster, new ConnectionLifecycleEventArgs(client));

        public void Handle(Cluster cluster, ConnectionLifecycleEventArgs args)
            => _handler(cluster, args);
    }

    public static partial class Extensions
    {
        public static ClusterEventHandlers ConnectionAdded(this ClusterEventHandlers handlers, Action<Cluster, ConnectionLifecycleEventArgs> handler)
        {
            handlers.Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Added, handler));
            return handlers;
        }

        public static ClusterEventHandlers ConnectionRemoved(this ClusterEventHandlers handlers, Action<Cluster, ConnectionLifecycleEventArgs> handler)
        {
            handlers.Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Removed, handler));
            return handlers;
        }
    }
}