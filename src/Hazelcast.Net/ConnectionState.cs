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

namespace Hazelcast
{
    /// <summary>
    /// Defines the possible states of the client connection.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// The client is not connected.
        /// </summary>
        NotConnected = 1, // zero is for default, make sure we start at 1

        /// <summary>
        /// The client is connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// The client is connected.
        /// </summary>
        Connected,

        /// <summary>
        /// The client has been disconnected and is reconnecting.
        /// </summary>
        Reconnecting
    }
}
