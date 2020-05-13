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

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Represents the networking configuration.
    /// </summary>
    public class NetworkingConfiguration
    {
        /// <summary>
        /// Gets or sets the list initial addresses.
        /// </summary>
        /// <remarks>
        /// <para>The client uses this list to find a running member and connect
        /// to it. This initial member will then send the list of other members
        /// to the client.</para>
        /// </remarks>
        public IList<string> Addresses { get; set; } = new List<string>();

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

        public bool RedoOperation { get; set; } = true;

        /// <summary>
        /// Gets or sets the connection timeout in milliseconds.
        /// </summary>
        /// <remarks>
        /// <para></para> FIXME: timeout for ?
        /// </remarks>
        public int ConnectionTimeoutMilliseconds { get; set; } = 5*1000;

        /// <summary>
        /// Gets or sets the reconnection mode in case the client is disconnected.
        /// </summary>
        public ReconnectMode ReconnectMode { get; set; } = ReconnectMode.ReconnectSync;

        /// <summary>
        /// Gets the SSL configuration.
        /// </summary>
        public SslConfiguration SslConfiguration { get; } = new SslConfiguration();

        /// <summary>
        /// Gets or sets the cloud configuration.
        /// </summary>
        public CloudConfiguration CloudConfiguration { get; } = new CloudConfiguration();

        /// <summary>
        /// Gets or sets the socket options.
        /// </summary>
        public SocketOptions SocketOptions { get; } = new SocketOptions();

        /// <summary>
        /// FIXME what is this + why strings?
        /// </summary>
        public ISet<string> OutboundPorts { get; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the socket interceptor configuration.
        /// </summary>
        public SocketInterceptorConfiguration SocketInterceptor { get; } = new SocketInterceptorConfiguration();

        /// <summary>
        /// Gets the connection retry configuration.
        /// </summary>
        public ConnectionRetryConfiguration ConnectionRetry { get; } = new ConnectionRetryConfiguration();
     }
}