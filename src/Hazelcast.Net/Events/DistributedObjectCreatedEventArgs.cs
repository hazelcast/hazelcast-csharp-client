﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    /// Represents event data for a cluster object created event.
    /// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix - here it is correct
    public sealed class DistributedObjectCreatedEventArgs : DistributedObjectLifecycleEventArgs
#pragma warning restore CA1711
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectCreatedEventArgs"/> class.
        /// </summary>
        /// <param name="serviceName">The service unique name.</param>
        /// <param name="name">The object unique name.</param>
        /// <param name="sourceMemberId">The unique identifier of the source member.</param>
        public DistributedObjectCreatedEventArgs(string serviceName, string name, Guid sourceMemberId)
            : base(serviceName, name, sourceMemberId)
        { }
    }
}
