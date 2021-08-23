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

using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Serialization;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides the messaging services of a cluster.
    /// </summary>
    internal class ClusterMessaging
    {
        private readonly ClusterState _clusterState;
        private readonly ClusterMembers _clusterMembers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterMessaging"/> class.
        /// </summary>
        /// <param name="clusterState">The cluster state.</param>
        /// <param name="clusterMembers">The cluster members.</param>
        public ClusterMessaging(ClusterState clusterState, ClusterMembers clusterMembers)
        {
            _clusterState = clusterState;
            _clusterMembers = clusterMembers;

            HConsole.Configure(x => x.Configure<ClusterMessaging>().SetPrefix("MSGING"));
        }

        /// <summary>
        /// Sends a message to a random member.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async Task<ClientMessage> SendAsync(ClientMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return await SendAsyncInternal(message, null, -1, default, cancellationToken).CfAwait();
        }

        /// <summary>
        /// Sends a message to a member.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        /// <remarks>
        /// <para>If <paramref name="memberId"/> is the default value, sends the message to a random member. If it
        /// is an unknown member, sends the message to a random number too.</para>
        /// </remarks>
        public async Task<ClientMessage> SendToMemberAsync(ClientMessage message, Guid memberId, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return await SendAsyncInternal(message, null, -1, memberId, cancellationToken).CfAwait();
        }

        /// <summary>
        /// Sends a message to a member.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="memberConnection">The member connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async Task<ClientMessage> SendToMemberAsync(ClientMessage message, MemberConnection memberConnection, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (memberConnection == null) throw new ArgumentNullException(nameof(memberConnection));

            return await SendAsyncInternal(message, memberConnection, -1, default, cancellationToken).CfAwait();
        }

        /// <summary>
        /// Sends a message to a member.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="memberConnection">The member connection.</param>
        /// <param name="correlationId">A correlation identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async Task<ClientMessage> SendToMemberAsync(ClientMessage message, MemberConnection memberConnection, long correlationId, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (memberConnection == null) throw new ArgumentNullException(nameof(memberConnection));

            return await SendAsyncInternal(message, memberConnection, -1, default, correlationId, cancellationToken).CfAwait();
        }

        /// <summary>
        /// Sends a message to the member owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendToKeyPartitionOwnerAsync(ClientMessage message, IData key, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (key == null) throw new ArgumentNullException(nameof(key));

            var partitionId = _clusterState.Partitioner.GetPartitionId(key.PartitionHash);
            if (partitionId < 0) throw new ArgumentException("Could not get a partition for this key.", nameof(key));

            message.PartitionId = partitionId;
            var task = SendAsyncInternal(message, null, partitionId, default, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Sends a message to the member owning a partition.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendToPartitionOwnerAsync(ClientMessage message, int partitionId, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (partitionId < 0) throw new ArgumentOutOfRangeException(nameof(partitionId));

            message.PartitionId = partitionId;
            var task = SendAsyncInternal(message, null, partitionId, default, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="connection">An optional target client.</param>
        /// <param name="targetPartitionId">An optional target partition identifier.</param>
        /// <param name="targetMemberId">An optional target member identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        private
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendAsyncInternal(ClientMessage message, MemberConnection connection, int targetPartitionId, Guid targetMemberId, CancellationToken cancellationToken = default)
        {
            var task = SendAsyncInternal(message, connection, targetPartitionId, targetMemberId, _clusterState.GetNextCorrelationId(), cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="connection">An optional target member connection.</param>
        /// <param name="targetPartitionId">An optional target partition identifier.</param>
        /// <param name="targetMemberId">An optional target member identifier.</param>
        /// <param name="correlationId">A correlation identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response message.</returns>
        private async Task<ClientMessage> SendAsyncInternal(ClientMessage message, MemberConnection connection, int targetPartitionId, Guid targetMemberId, long correlationId, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // fail fast, if the cluster is not active
            _clusterState.ThrowIfNotActive();

            // assign a unique identifier to the message
            // and send in one fragment, with proper flags
            message.CorrelationId = correlationId;
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // create the invocation
            var invocation = connection != null ? new Invocation(message, _clusterState.Options.Messaging, connection) :
                             targetPartitionId >= 0 ? new Invocation(message, _clusterState.Options.Messaging, targetPartitionId) :
                             targetMemberId != default ? new Invocation(message, _clusterState.Options.Messaging, targetMemberId) :
                             new Invocation(message, _clusterState.Options.Messaging);

            return await SendAsyncInternal(invocation, cancellationToken).CfAwait();
        }

        /// <summary>
        /// Sends an invocation request message.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response message.</returns>
        private async Task<ClientMessage> SendAsyncInternal(Invocation invocation, CancellationToken cancellationToken)
        {
            // yield now, so the caller gets a task that can bubble up to user's code
            // immediately without waiting for more synchronous operations to take place
            await Task.Yield();

            // NOTE: *every* invocation sent to the cluster goes through the code below

            // if the client is active but not connected, we are going to retry the invocation for a while (assuming it
            // is retryable) and then timeout and bubble the invocation to the user - and we do *not* have a way to sort
            // of suspend the whole client waiting for it to reconnect, because... what sense does it make? That would
            // be the "sync" reconnect mode - TODO: figure this out

            while (true)
            {
                try
                {
                    var connection = GetInvocationConnection(invocation); // non-null, throws if no connections
                    return await connection.SendAsync(invocation, cancellationToken).CfAwait();
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    // if the client is not active, die - an active client is starting, started, connected or
                    // disconnected - but attempting to reconnect - whereas a non-active client is down and
                    // will not go back up
                    _clusterState.ThrowIfNotActive(exception);

                    // if the invocation is not retryable, throw
                    if (!invocation.IsRetryable(exception, _clusterState.Options.Networking.RedoOperations))
                        throw;

                    // else, wait for retrying
                    // this will throw if it cannot retry
                    await invocation.WaitRetryAsync(() => _clusterState.GetNextCorrelationId(), cancellationToken).CfAwait();

                    HConsole.WriteLine(this, "Retrying...");
                }
            }
        }

        /// <summary>
        /// Gets a connection for an invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <returns>A connection for the invocation.</returns>
        /// <exception cref="ClientOfflineException">Occurs when no connection could be found.</exception>
        private MemberConnection GetInvocationConnection(Invocation invocation)
        {
            // try the target connection
            var connection = invocation.TargetClientConnection;
            if (connection != null) return connection;

            // try the partition
            if (invocation.TargetPartitionId >= 0)
            {
                var memberId = _clusterState.Partitioner.GetPartitionOwner(invocation.TargetPartitionId);
                if (_clusterMembers.TryGetConnection(memberId, out connection))
                    return connection;
            }

            // try the member
            if (invocation.TargetMemberId != default)
            {
                if (_clusterMembers.TryGetConnection(invocation.TargetMemberId, out connection))
                    return connection;
            }

            // fall over to random client
            connection = _clusterMembers.GetRandomConnection();
            if (connection != null)
                return connection;

            // fail
            throw _clusterState.ThrowClientOfflineException();
        }
    }
}
