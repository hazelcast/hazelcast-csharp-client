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
using Hazelcast.Configuration;
using Hazelcast.Core;

namespace Hazelcast.Networking
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
        /// Gets or sets the SSL configuration.
        /// </summary>
        public SslConfiguration Ssl { get; set;  } = new SslConfiguration();

        /// <summary>
        /// Gets or sets the cloud configuration.
        /// </summary>
        public CloudConfiguration Cloud { get; set;  } = new CloudConfiguration();

        /// <summary>
        /// Gets or sets the socket options.
        /// </summary>
        public SocketOptions SocketOptions { get; set;  } = new SocketOptions();

        /// <summary>
        /// FIXME what is this + why strings?
        /// </summary>
        public ISet<string> OutboundPorts { get; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the socket interceptor configuration.
        /// </summary>
        public SocketInterceptorConfiguration SocketInterceptor { get; set;  } = new SocketInterceptorConfiguration();

        /// <summary>
        /// Gets the connection retry configuration.
        /// </summary>
        public RetryConfiguration ConnectionRetry { get; } = new RetryConfiguration();

        /// <summary>
        /// Parses configuration from an Xml document.
        /// </summary>
        /// <param name="node">The Xml node.</param>
        /// <returns>The configuration.</returns>
        public static NetworkingConfiguration Parse(XmlNode node)
        {
            var configuration = new NetworkingConfiguration();

            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = child.GetCleanName();
                switch (nodeName)
                {
                    case "cluster-members":
                        foreach (XmlNode child2 in child.ChildNodes)
                        {
                            if ("address".Equals(child2.GetCleanName()))
                            {
                                configuration.Addresses.Add(child2.GetTextContent());
                            }
                        }
                        break;
                    case "smart-routing":
                        configuration.SmartRouting = child.GetBoolContent();
                        break;
                    case "redo-operation":
                        configuration.RedoOperation = child.GetBoolContent();
                        break;
                    case "connection-timeout":
                        configuration.ConnectionTimeoutMilliseconds = child.GetInt32Content();
                        break;
                    case "socket-options":
                        configuration.SocketOptions = SocketOptions.Parse(child);
                        break;
                    case "ssl":
                        configuration.Ssl = SslConfiguration.Parse(child);
                        break;
                    case "hazelcast-cloud":
                        configuration.Cloud = CloudConfiguration.Parse(child);
                        break;
                    case "socket-interceptor":
                        configuration.SocketInterceptor = SocketInterceptorConfiguration.Parse(child);
                        break;
                }
            }

            return configuration;
        }
    }
}