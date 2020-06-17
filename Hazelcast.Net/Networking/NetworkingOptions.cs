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
            RetryOnTargetDisconnected = other.RetryOnTargetDisconnected;
            ConnectionTimeoutMilliseconds = other.ConnectionTimeoutMilliseconds;
            WaitForClientMilliseconds = other.WaitForClientMilliseconds;
            ReconnectMode = other.ReconnectMode;

            Ssl = other.Ssl.Clone();
            Cloud = other.Cloud.Clone();
            Socket = other.Socket.Clone();
            SocketInterception = other.SocketInterception.Clone();
            ConnectionRetry = other.ConnectionRetry.Clone();
        }

        /// <summary>
        /// Gets or sets the list initial addresses.
        /// </summary>
        /// <remarks>
        /// <para>The client uses this list to find a running member and connect
        /// to it. This initial member will then send the list of other members
        /// to the client.</para>
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
        /// Whether to redo operations.
        /// </summary>
        /// <remarks>
        /// <para>If true, the client will redo the operations that were executing on the server and
        /// the client lost the connection. This can happen because of network, or simply because the
        /// member died.</para>
        /// <para>Note that it is not clear whether the operation is performed or not. For idempotent
        /// operations this is harmless, but for non idempotent ones retrying can cause
        /// undesirable effects. Also note that the redo can perform on any member.</para>
        /// </remarks>

        public bool RetryOnTargetDisconnected { get; set; } = true;

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        /// <remarks>
        /// <para>Specifies the Hazelcast client connection timeout, i.e. the maximum amount of time
        /// the Hazelcast can spend trying to establish a connection to the cluster. See <see cref="SocketOptions"/>
        /// for specifying the individual socket connection timeout.</para>
        /// </remarks>
        public int ConnectionTimeoutMilliseconds { get; set; } = 60_000;

        /// <summary>
        /// Gets or sets the reconnection mode in case the client is disconnected.
        /// </summary>
        public ReconnectMode ReconnectMode { get; set; } = ReconnectMode.ReconnectSync;

        /// <summary>
        /// Gets or sets the delay to pause for when looking for a client
        /// to handle cluster view events and no client is available.
        /// </summary>
        public int WaitForClientMilliseconds { get; set; } = 1_000;

        /// <summary>
        /// Gets the SSL options.
        /// </summary>
        public SslOptions Ssl { get; } = new SslOptions();

        /// <summary>
        /// Gets the cloud options.
        /// </summary>
        public CloudOptions Cloud { get; } = new CloudOptions();

        /// <summary>
        /// Gets the socket options.
        /// </summary>
        public SocketOptions Socket { get; } = new SocketOptions();

        /// <summary>
        /// Gets the socket interceptor options.
        /// </summary>
        public SocketInterceptionOptions SocketInterception { get; } = new SocketInterceptionOptions();

        /// <summary>
        /// Gets the connection retry options.
        /// </summary>
        public RetryOptions ConnectionRetry { get; } = new RetryOptions();

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal NetworkingOptions Clone() => new NetworkingOptions(this);
    }
}
