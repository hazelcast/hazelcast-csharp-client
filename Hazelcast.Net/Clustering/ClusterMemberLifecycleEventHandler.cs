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
    internal class ClusterMemberLifecycleEventHandler : IClusterEventHandler
    {
        private readonly Action<Cluster, ClusterMemberLifecycleEventArgs> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterObjectLifecycleEventHandler"/> class.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="handler">An action to execute</param>
        public ClusterMemberLifecycleEventHandler(ClusterMemberLifecycleEventType eventType, Action<Cluster, ClusterMemberLifecycleEventArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public ClusterMemberLifecycleEventType EventType { get; }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="member">The member.</param>
        /// <param name="serviceName">The unique name of the service managing the object.</param>
        /// <param name="name">The unique name of the object.</param>
        public void Handle(Cluster sender, MemberInfo member)
            => _handler(sender, CreateEventArgs(member));

        public void Handle(Cluster sender, ClusterMemberLifecycleEventArgs args)
            => _handler(sender, args);

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>Event arguments.</returns>
        private static ClusterMemberLifecycleEventArgs CreateEventArgs(MemberInfo member)
            => new ClusterMemberLifecycleEventArgs(member);
    }
}
