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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Clustering
{
    internal class ClusterMemberLifecycleEventHandler : ClusterEventHandlerBase<ClusterMemberLifecycleEventArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterObjectLifecycleEventHandler"/> class.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="handler">An action to execute</param>
        public ClusterMemberLifecycleEventHandler(ClusterMemberLifecycleEventType eventType, Func<Cluster, ClusterMemberLifecycleEventArgs, CancellationToken, ValueTask> handler)
            : base(handler)
        {
            EventType = eventType;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public ClusterMemberLifecycleEventType EventType { get; }
    }
}
