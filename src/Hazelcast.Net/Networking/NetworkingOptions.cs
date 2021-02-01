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

using System.Collections.Generic;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents the networking options.
    /// </summary>
    public class NetworkingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkingOptions"/> class.
        /// </summary>
        public NetworkingOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkingOptions"/> class.
        /// </summary>
        private NetworkingOptions(NetworkingOptions other)
        {
            Addresses = new List<string>(other.Addresses);
            ShuffleAddresses = other.ShuffleAddresses;
            SmartRouting = other.SmartRouting;
            RedoOperations = other.RedoOperations;
            ReconnectMode = other.ReconnectMode;
            ConnectionTimeoutMilliseconds = other.ConnectionTimeoutMilliseconds;

            Ssl = other.Ssl.Clone();
            Cloud = other.Cloud.Clone();
            Socket = other.Socket.Clone();
            ConnectionRetry = other.ConnectionRetry.Clone();
        }

        /// <summary>
        /// Gets the default Hazelcast server port.
        /// </summary>
        internal int DefaultPort { get; } = 5701;

        /// <summary>
        /// Gets the port range to scan.
        /// </summary>
        internal int PortRange { get; } = 3;

        /// <summary>
        /// Gets or sets the list of initial addresses.
        /// </summary>
        /// <remarks>
        /// <para>The client uses this list to find a running member and connect
        /// to it. This initial member will then send the list of other members
        /// to the client.</para>
        /// <para>Each address must be a valid IPv4 or IPv6 address.</para>
        /// </remarks>
        public IList<string> Addresses { get; } = new List<string>();

        /// <summary>
        /// Whether to shuffle addresses when attempting to connect to the cluster.
        /// </summary>
        public bool ShuffleAddresses { get; set; } = true;

        /// <summary>
        /// Whether smart routing is enabled.
        /// </summary>
        /// <remarks>
        /// <para>If true (default), client will route the key based operations to owner of
        /// the key at the best effort.</para>
        /// <para>Note that it however does not guarantee that the operation will always be
        /// executed on the owner, as the member table is only updated every 10 seconds.</para>
        /// </remarks>
        public bool SmartRouting { get; set; } = true;

        /// <summary>
        /// Whether to retry operations.
        /// </summary>
        /// <remarks>
        /// <para>While sending the requests to related members, operations can fail due to various
        /// reasons. Read-only operations are retried by default. If you want to enable retry for
        /// the other operations, set this property to <c>true</c>.</para>
        /// <para>Note that it is not clear whether the operation is performed or not. For idempotent
        /// operations this is harmless, but for non idempotent ones retrying can cause
        /// undesirable effects. Also note that the redo can perform on any member.</para>
        /// </remarks>

        public bool RedoOperations { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="ReconnectMode"/> in case the client is disconnected.
        /// </summary>
        public ReconnectMode ReconnectMode { get; set; } = ReconnectMode.DoNotReconnect;

        /// <summary>
        /// Gets the <see cref="SslOptions"/>.
        /// </summary>
        public SslOptions Ssl { get; } = new SslOptions();

        /// <summary>
        /// Gets the <see cref="CloudOptions"/>.
        /// </summary>
        public CloudOptions Cloud { get; } = new CloudOptions();

        /// <summary>
        /// Gets the <see cref="SocketOptions"/>.
        /// </summary>
        public SocketOptions Socket { get; } = new SocketOptions();

        /// <summary>
        /// Gets the connection <see cref="ConnectionRetryOptions"/>.
        /// </summary>
        /// <remarks>
        /// <para>Specifies the Hazelcast client connection parameters, including the timeout, i.e. the maximum
        /// amount of time that the Hazelcast client can spend trying to establish a connection to the cluster
        /// before failing. See <see cref="SocketOptions"/> for specifying the individual socket parameters,
        /// including the individual socket connection timeout.</para>
        /// </remarks>
        public ConnectionRetryOptions ConnectionRetry { get; } = new ConnectionRetryOptions();

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        /// <remarks>
        /// <para>This timeout is used in various places. It is the connection timeout for each individual
        /// socket. It is also the timeout for cloud discovery.</para>
        /// </remarks>
        public int ConnectionTimeoutMilliseconds { get; set; } = 5_000;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal NetworkingOptions Clone() => new NetworkingOptions(this);
    }
}
