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
        /// The client state is unknown.
        /// </summary>
        /// <remarks>
        /// <para>This value is used to represent an unknown value of the state.</para>
        /// </remarks>
        Unknown = 0,

        /// <summary>
        /// The client is not connected.
        /// </summary>
        /// <remarks>
        /// <para>The client will transition to <see cref="Connecting"/> when requested
        /// to connect by the user.</para>
        /// </remarks>
        NotConnected = 1, // zero is for default, make sure we start at 1

        /// <summary>
        /// The client is connecting.
        /// </summary>
        /// <remarks>
        /// <para>The client will transition to <see cref="Connected"/> when it
        /// has successfully connected, or to <see cref="NotConnected"/> in case it fails
        /// to connect.</para>
        /// </remarks>
        Connecting,

        /// <summary>
        /// The client is connected.
        /// </summary>
        /// <remarks>
        /// <para>The client will remain connected as long as it is not required to
        /// disconnect by the user (in which case it will transition to <see cref="Disconnecting"/>
        /// or disconnected by the server or the network (in which case in will
        /// transition to <see cref="Disconnected"/>.</para>
        /// </remarks>
        Connected,

        /// <summary>
        /// The client has been disconnected.
        /// </summary>
        /// <remarks>
        /// <para>This is a very temporary state. The client will transition to
        /// <see cref="Reconnecting"/> if it is configured to reconnect when disconnected,
        /// or to <see cref="NotConnected"/> otherwise.</para>
        /// </remarks>
        Disconnected,

        /// <summary>
        /// The client has been disconnected and is reconnecting.
        /// </summary>
        /// <remarks>
        /// <para>The client will transition to <see cref="Connected"/> when it
        /// has successfully reconnected, or to <see cref="NotConnected"/> in case it fails
        /// to reconnect.</para>
        /// </remarks>
        Reconnecting,

        /// <summary>
        /// The client is disconnecting.
        /// </summary>
        /// <remarks>
        /// <para>The client will transition to <see cref="NotConnected"/> when it
        /// is done disconnecting.</para>
        /// </remarks>
        Disconnecting
    }
}
