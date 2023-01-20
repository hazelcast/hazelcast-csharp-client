// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
        /// The client is starting.
        /// </summary>
        /// <remarks>
        /// <para>The client will transition to <see cref="Started"/> when it has started.</para>
        /// </remarks>
        Starting,

        /// <summary>
        /// The client has started, and is now trying to connect to a first member.
        /// </summary>
        /// <remarks>
        /// <para>The client will transition to <see cref="Connected"/> when it
        /// has successfully connected, or to <see cref="Shutdown"/> in case it fails
        /// to connect.</para>
        /// <para>In a failover scenario, the client may try more than one cluster
        /// until it manages to establish a first connection.</para>
        /// <para>In the case where the client cannot immediately connect to the first cluster
        /// of the failover list and has to try other clusters, it will transition to <see cref="ClusterChanged"/>
        /// when it manages to connect to a cluster, even if it is the first cluster of the list.</para>
        /// </remarks>
        Started,

        /// <summary>
        /// The client is connected.
        /// </summary>
        /// <remarks>
        /// <para>The client will remain connected as long as it is not required to
        /// disconnect by the user (in which case it will transition to <see cref="ShuttingDown"/>)
        /// or disconnected by the server or the network (in which case in will
        /// transition to <see cref="Disconnected"/>).</para>
        /// </remarks>
        Connected,

        /// <summary>
        /// The client has been disconnected.
        /// </summary>
        /// <remarks>
        /// <para>Depending on the configuration, the client may try to reconnect, to the
        /// same cluster or (if failover is enabled) to other clusters. If it can successfully
        /// reconnect to the same cluster, it transitions back to <see cref="Connected"/>.
        /// Alternatively, if failover is enabled, it will try to connect to other clusters and,
        /// if successful, transition to <see cref="ClusterChanged"/>.
        /// If all reconnection attempts fail, it transitions to <see cref="Shutdown"/>.</para>
        /// <para>Note that if failover is involved, the client will transition to
        /// <see cref="ClusterChanged"/> if it can reconnect to any cluster, and that includes
        /// the original cluster it was connected to. For instance, after being disconnected
        /// from A, the client tries to reconnect to A, then fails over to B, then C, then
        /// A again and succeeds: this is considered a cluster change.</para>
        /// </remarks>
        Disconnected,

        /// <summary>
        /// The client is shutting down.
        /// </summary>
        /// <remarks>
        /// <para>This state is reached only when the client is properly requested to shut
        /// down. If the client ends up being disconnected, for instance due to a network
        /// problem, and cannot reconnect, it will directly transition to <see cref="Shutdown"/>.</para>
        /// <para>The client will transition to <see cref="Shutdown"/> once shutdown is complete.</para>
        /// </remarks>
        ShuttingDown,

        /// <summary>
        /// The client, which was <see cref="Disconnected"/>, is about to reconnect to a different cluster.
        /// </summary>
        /// <remarks>
        /// <para>This state is reached when a <see cref="Disconnected"/> client managed to
        /// establish a connection to a member of a different cluster, in a failover scenario. It
        /// is also reached when a <see cref="Started"/> client failed to immediately connect to
        /// the first cluster of a failover scenario, and had to try more clusters.</para>
        /// <para>As soon as a client enters a failover scenario, it will enter <see cref="ClusterChanged"/>
        /// before entering <see cref="Connected"/>, even though it ends up connecting to the first
        /// cluster, or re-connecting to the original cluster.</para>
        /// <para>The client performs some house-keeping tasks, then transitions to <see cref="Connected"/>.</para>
        /// </remarks>
        ClusterChanged, // should be 'ChangingCluster' really but we have to stick with Java's names

        /// <summary>
        /// The client has shut down.
        /// </summary>
        /// <remarks>
        /// <para>This is the final, terminal state.</para>
        /// </remarks>
        Shutdown
    }
}
