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
    internal enum ConnectionLifecycleEventType
    {
        Added,
        Removed
    }

    public class ConnectionLifecycleEventArgs
    {
        public ConnectionLifecycleEventArgs(ClientSocketConnection connection)
        {
            Connection = connection;
        }

        public ClientSocketConnection Connection { get; }
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

        public void Handle(Cluster cluster, ClientSocketConnection connection)
            => _handler(cluster, new ConnectionLifecycleEventArgs(connection));

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