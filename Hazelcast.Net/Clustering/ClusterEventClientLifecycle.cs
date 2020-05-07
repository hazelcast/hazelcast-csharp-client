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

namespace Hazelcast.Clustering
{
    internal enum ClientLifecycleEventType
    {
        Added,
        Removed
    }

    public class ClientLifecycleEventArgs
    { }

    internal class ClientLifecycleEventHandler : IClusterEventHandler
    {
        public ClientLifecycleEventHandler(ClientLifecycleEventType eventType, Action<Cluster, ClientLifecycleEventArgs> handler)
        {}
    }

    public static partial class Extensions
    {
        public static ClusterEvents ClientAdded(this ClusterEvents events, Action<Cluster, ClientLifecycleEventArgs> handler)
        {
            events.Handlers.Add(new ClientLifecycleEventHandler(ClientLifecycleEventType.Added, handler));
            return events;
        }

        public static ClusterEvents ClientRemoved(this ClusterEvents events, Action<Cluster, ClientLifecycleEventArgs> handler)
        {
            events.Handlers.Add(new ClientLifecycleEventHandler(ClientLifecycleEventType.Removed, handler));
            return events;
        }
    }
}