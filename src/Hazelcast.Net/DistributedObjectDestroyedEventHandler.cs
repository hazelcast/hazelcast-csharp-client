// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Events;

namespace Hazelcast
{
    /// <summary>
    /// Represents a handler for a cluster object destroyed event.
    /// </summary>
    internal class DistributedObjectDestroyedEventHandler : HazelcastClientEventHandlerBase<DistributedObjectDestroyedEventArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectCreatedEventHandler"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public DistributedObjectDestroyedEventHandler(Func<IHazelcastClient, DistributedObjectDestroyedEventArgs, ValueTask> handler)
            : base(handler)
        { }
    }
}
