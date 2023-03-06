// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents the result of the client authentication.
    /// </summary>
    internal sealed class AuthenticationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
        /// </summary>
        /// <param name="clusterId">The unique identifier of the cluster.</param>
        /// <param name="memberId">The unique identifier of the member.</param>
        /// <param name="memberAddress">The network address of the member.</param>
        /// <param name="serverVersion">The version of the server running the member.</param>
        /// <param name="failoverSupported">Whether fail-over is supported.</param>
        /// <param name="partitionCount">The partition count.</param>
        /// <param name="serializationVersion">The serialization version.</param>
        /// <param name="principal">The principal that was used to authenticate.</param>
        public AuthenticationResult(Guid clusterId, Guid memberId, NetworkAddress memberAddress, string serverVersion, bool failoverSupported, int partitionCount, byte serializationVersion, string principal)
        {
            ClusterId = clusterId;
            MemberId = memberId;
            MemberAddress = memberAddress;
            ServerVersion = serverVersion;
            FailoverSupported = failoverSupported;
            PartitionCount = partitionCount;
            SerializationVersion = serializationVersion;
            Principal = principal;
        }

        /// <summary>
        /// Gets the unique identifier of the cluster.
        /// </summary>
        public Guid ClusterId { get; }

        /// <summary>
        /// Gets the unique identifier of the member.
        /// </summary>
        public Guid MemberId { get; }

        /// <summary>
        /// Gets the name of the principal that was used to authenticate.
        /// </summary>
        public string Principal { get; }

        /// <summary>
        /// Gets the network address of the member.
        /// </summary>
        public NetworkAddress MemberAddress { get; }

        /// <summary>
        /// Gets the version of the server running the member.
        /// </summary>
        public string ServerVersion { get; }

        /// <summary>
        /// Determines whether fail-over is supported.
        /// </summary>
        public bool FailoverSupported { get; }

        /// <summary>
        /// Gets the partition count.
        /// </summary>
        public int PartitionCount { get; }

        /// <summary>
        /// Gets the serialization version.
        /// </summary>
        public byte SerializationVersion { get; }
    }
}
