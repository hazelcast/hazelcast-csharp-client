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
    public enum ClientLifecycleState
    {
        Starting,
        Started,
        ShuttingDown,
        Shutdown,
        Connected,
        Disconnected
    }

    public class ClientLifecycleEventArgs
    {
        public ClientLifecycleEventArgs(ClientLifecycleState state)
        {
            State = state;
        }

        /// <summary>
        /// Gets the new state.
        /// </summary>
        public ClientLifecycleState State { get; }
    }

    internal class ClientLifecycleEventHandler : IClusterEventHandler
    {
        private readonly Action<Cluster, ClientLifecycleEventArgs> _handler;

        public ClientLifecycleEventHandler(Action<Cluster, ClientLifecycleEventArgs> handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="sender">The originating cluster.</param>
        /// <param name="state">The new state.</param>
        public void Handle(Cluster sender, ClientLifecycleState state)
            => _handler(sender, new ClientLifecycleEventArgs(state));

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="sender">The originating cluster.</param>
        /// <param name="args">The event arguments.</param>
        public void Handle(Cluster sender, ClientLifecycleEventArgs args)
            => _handler(sender, args);
    }

    public static partial class Extensions
    {
        // TODO: original code has 1 unique 'StateChanged' event, consider having 1 event per new state?
        // eg ClientStarting, ClientStarted, etc...?

        public static ClusterEvents ClientStateChanged(this ClusterEvents events, Action<Cluster, ClientLifecycleEventArgs> handler)
        {
            events.Handlers.Add(new ClientLifecycleEventHandler(handler));
            return events;
        }
    }
}