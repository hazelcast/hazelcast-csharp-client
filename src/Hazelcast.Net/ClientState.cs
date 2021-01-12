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

namespace Hazelcast
{
    /// <summary>
    /// Defines the possible states of the client.
    /// </summary>
    public enum ClientState
    {
        /// <summary>
        /// The client is new and starting.
        /// </summary>
        /// <remarks>
        /// <para>The client will transition to <see cref="Started"/> when it has started.</para>
        /// </remarks>
        Starting, // zero is the default value, make sure real states start at 1

        /// <summary>
        /// The client has started, and is now trying to connect to a first member.
        /// </summary>
        /// <para>The client will transition to <see cref="Connected"/> when it
        /// has successfully connected, or to <see cref="Shutdown"/> in case it fails
        /// to connect.</para>
        Started,

        /// <summary>
        /// The client is connected.
        /// </summary>
        /// <remarks>
        /// <para>The client will remain connected as long as it is not required to
        /// disconnect by the user (in which case it will transition to <see cref="ShuttingDown"/>
        /// or disconnected by the server or the network (in which case in will
        /// transition to <see cref="Disconnected"/>.</para>
        /// </remarks>
        Connected,

        /// <summary>
        /// The client has been disconnected.
        /// </summary>
        /// <remarks>
        /// <para>Depending on the configuration, the client may try to reconnected, and
        /// if successful, transition back to <see cref="Connected"/>. Otherwise, it
        /// will transition to <see cref="Shutdown"/>.</para>
        /// </remarks>
        Disconnected,

        /// <summary>
        /// The client is shutting down.
        /// </summary>
        /// <remarks>
        /// <para>The client will transition to <see cref="Shutdown"/> once shutdown is complete.</para>
        /// </remarks>
        ShuttingDown,

        /// <summary>
        /// The client has shut down.
        /// </summary>
        /// <remarks>
        /// <para>This is the final, terminal state.</para>
        /// </remarks>
        Shutdown
    }
}
