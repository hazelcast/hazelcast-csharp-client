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
using Hazelcast.Data;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a handler for a cluster object lifecycle event.
    /// </summary>
    internal class ClusterObjectLifecycleEventHandler : IClusterEventHandler
    {
        private readonly Action<Cluster, ClusterObjectLifecycleEventArgs> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterObjectLifecycleEventHandler"/> class.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="handler">An action to execute</param>
        public ClusterObjectLifecycleEventHandler(ClusterObjectLifecycleEventType eventType, Action<Cluster, ClusterObjectLifecycleEventArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public ClusterObjectLifecycleEventType EventType { get; }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="sender">The originating cluster.</param>
        /// <param name="member">The member.</param>
        /// <param name="serviceName">The unique name of the service managing the object.</param>
        /// <param name="name">The unique name of the object.</param>
        public void Handle(Cluster sender, MemberInfo member, string serviceName, string name)
            => _handler(sender, CreateEventArgs(member, serviceName, name));

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="sender">The originating cluster.</param>
        /// <param name="args">The event arguments.</param>
        public void Handle(Cluster sender, ClusterObjectLifecycleEventArgs args)
            => _handler(sender, args);

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="serviceName">The unique name of the service managing the object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <returns>Event arguments.</returns>
        private static ClusterObjectLifecycleEventArgs CreateEventArgs(MemberInfo member, string serviceName, string name)
            => new ClusterObjectLifecycleEventArgs(serviceName, name, member.Id);
    }
}
