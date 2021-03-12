// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Events
{
    /// <summary>
    /// Provides a base class for cluster object lifecycle events data.
    /// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix - here it is correct
    public abstract class DistributedObjectLifecycleEventArgs
#pragma warning restore CA1711
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectLifecycleEventArgs"/> class.
        /// </summary>
        /// <param name="serviceName">The service unique name.</param>
        /// <param name="name">The object unique name.</param>
        /// <param name="sourceMemberId">The unique identifier of the source member.</param>
        protected DistributedObjectLifecycleEventArgs(string serviceName, string name, Guid sourceMemberId)
        {
            ServiceName = serviceName;
            Name = name;
            SourceMemberId = sourceMemberId;
        }

        /// <summary>
        /// Gets the name of the service handling the impacted object.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Gets the name of the impacted object.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the unique identifier of the source member.
        /// </summary>
        public Guid SourceMemberId { get; }
    }
}
