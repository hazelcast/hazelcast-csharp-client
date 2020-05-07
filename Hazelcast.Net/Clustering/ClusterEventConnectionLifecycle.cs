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
    internal enum ConnectionLifecycleEventType
    {
        Added,
        Removed
    }

    public class ConnectionLifecycleEventArgs 
    { }

    internal class ConnectionLifecycleEventHandler : IClusterEventHandler
    {
        public ConnectionLifecycleEventHandler(ConnectionLifecycleEventType eventType, Action<Cluster, ConnectionLifecycleEventArgs> handler)
        {

        }
    }

    public static partial class Extensions
    {
        public static ClusterEvents ConnectionAdded(this ClusterEvents events, Action<Cluster, ConnectionLifecycleEventArgs> handler)
        {
            events.Handlers.Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Added, handler));
            return events;
        }

        public static ClusterEvents ConnectionRemoved(this ClusterEvents events, Action<Cluster, ConnectionLifecycleEventArgs> handler)
        {
            events.Handlers.Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Removed, handler));
            return events;
        }
    }
}