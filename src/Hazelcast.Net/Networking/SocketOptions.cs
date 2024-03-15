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

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents the TCP socket options.
    /// </summary>
    public class SocketOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketOptions"/> class.
        /// </summary>
        public SocketOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketOptions"/> class.
        /// </summary>
        private SocketOptions(SocketOptions other)
        {
            BufferSizeKiB = other.BufferSizeKiB;
            KeepAlive = other.KeepAlive;
            LingerSeconds = other.LingerSeconds;
            TcpNoDelay = other.TcpNoDelay;
        }

        /// <summary>
        /// The send and receive buffers size.
        /// </summary>
        /// <remarks>
        /// <para>The buffer size is expressed in Kibibytes, ie units of 1024 bytes. This
        /// sets the size of both the send and receive buffers.</para>
        /// </remarks>
        public int BufferSizeKiB { get; set; } = 128;

        /// <summary>
        /// Whether to keep the socket alive.
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// The number of seconds to remain connected after the socket Close() method is called, or zero to disconnect immediately.
        /// </summary>
        public int LingerSeconds { get; set; } = 3;

        /// <summary>
        /// Whether the socket is using the Nagle algorithm.
        /// </summary>
        public bool TcpNoDelay { get; set; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal SocketOptions Clone() => new SocketOptions(this);
    }
}
