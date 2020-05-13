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

using System;
using System.Xml;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents the TCP socket options.
    /// </summary>
    public class SocketOptions
    {
        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        public int BufferSize { get; set; } = 128; // TODO: bytes? kilobytes?

        /// <summary>
        /// Whether to keep the socket alive.
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// TODO: document
        /// </summary>
        public int LingerSeconds { get; set; } = 3;

        /// <summary>
        /// TODO: document
        /// </summary>
        public bool ReuseAddress { get; set; } = true;

        /// <summary>
        /// TODO: document
        /// </summary>
        public bool TcpNoDelay { get; set; }

        /// <summary>
        /// Parses configuration from an Xml document.
        /// </summary>
        /// <param name="node">The Xml node.</param>
        /// <returns>The configuration.</returns>
        public static SocketOptions Parse(XmlNode node)
        {
            var options = new SocketOptions();

            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = child.GetCleanName();
                switch (nodeName)
                {
                    case "tcp-no-delay":
                        options.TcpNoDelay = child.GetBoolContent();
                        break;
                    case "keep-alive":
                        options.KeepAlive = child.GetBoolContent();
                        break;
                    case "reuse-address":
                        options.ReuseAddress = child.GetBoolContent();
                        break;
                    case "linger-seconds":
                        options.LingerSeconds = child.GetInt32Content();
                        break;
                    case "buffer-size":
                        options.BufferSize = child.GetInt32Content();
                        break;
                }
            }

            return options;
        }
    }
}