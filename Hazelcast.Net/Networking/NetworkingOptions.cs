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
using System.Xml;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents the networking options.
    /// </summary>
    public class NetworkingOptions
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
        /// Gets or sets the SSL options.
        /// </summary>
        public SslOptions Ssl { get; private set;  } = new SslOptions();

        /// <summary>
        /// Gets or sets the cloud options.
        /// </summary>
        public CloudOptions Cloud { get; private set;  } = new CloudOptions();

        /// <summary>
        /// Gets or sets the socket options.
        /// </summary>
        public SocketOptions Socket { get; private set;  } = new SocketOptions();

        /// <summary>
        /// FIXME what is this + why strings?
        /// </summary>
        public ISet<string> OutboundPorts { get; private set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the socket interceptor options.
        /// </summary>
        public SocketInterceptorOptions SocketInterceptor { get; private set;  } = new SocketInterceptorOptions();

        /// <summary>
        /// Gets the connection retry options.
        /// </summary>
        public RetryOptions ConnectionRetry { get; private set; } = new RetryOptions();

        /// <summary>
        /// Clones the options.
        /// </summary>
        public NetworkingOptions Clone()
        {
            return new NetworkingOptions
            {
                Addresses = new List<string>(Addresses),
                SmartRouting = SmartRouting,
                RedoOperation = RedoOperation,
                ConnectionTimeoutMilliseconds = ConnectionTimeoutMilliseconds,
                ReconnectMode = ReconnectMode,
                Ssl = Ssl.Clone(),
                Cloud = Cloud.Clone(),
                Socket = Socket.Clone(),
                OutboundPorts = new HashSet<string>(OutboundPorts),
                SocketInterceptor = SocketInterceptor.Clone(),
                ConnectionRetry = ConnectionRetry.Clone()
            };
        }
    }
}
