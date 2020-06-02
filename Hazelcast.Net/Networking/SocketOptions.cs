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
        /// Clones the options.
        /// </summary>
        public SocketOptions Clone()
        {
            return new SocketOptions
            {
                BufferSize = BufferSize,
                KeepAlive = KeepAlive,
                LingerSeconds = LingerSeconds,
                ReuseAddress = ReuseAddress,
                TcpNoDelay = TcpNoDelay
            };
        }
    }
}
