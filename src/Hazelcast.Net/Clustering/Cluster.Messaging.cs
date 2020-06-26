﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    internal partial class Cluster // Messaging
    {
        /// <summary>
        /// Sends a message to a random target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async Task<ClientMessage> SendAsync(ClientMessage message, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            using var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            return await SendAsyncInternal(message, null, cancellation.Token).CAF();
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
        public async Task<ClientMessage> SendToMemberAsync(ClientMessage message, Guid memberId, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // try to get the specified member, else use a random member
            // connections to members are maintained elsewhere - we don't lazy-connect on demand
            if (memberId == default || !_clients.TryGetValue(memberId, out var client))
                client = GetRandomClient();

            if (client == null) throw new InvalidOperationException("Could not get a client.");

            using var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            return await SendAsyncInternal(message, client, cancellation.Token).CAF();
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">The target.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async Task<ClientMessage> SendToClientAsync(ClientMessage message, ClientConnection client, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (client == null) throw new ArgumentNullException(nameof(client));

            using var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            return await SendAsyncInternal(message, client, cancellation.Token).CAF();
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">The target.</param>
        /// <param name="correlationId">A correlation identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async Task<ClientMessage> SendToClientAsync(ClientMessage message, ClientConnection client, long correlationId, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (client == null) throw new ArgumentNullException(nameof(client));

            using var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            return await SendAsyncInternal(message, client, correlationId, cancellation.Token).CAF();
        }

        /// <summary>
        /// Sends a message to the target owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendToKeyPartitionOwnerAsync(ClientMessage message, IData key, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (key == null) throw new ArgumentNullException(nameof(key));

            var partitionId = Partitioner.GetPartitionId(key);
            if (partitionId < 0) throw new ArgumentException("Could not get a partition for this key.", nameof(key));

            var task = SendToPartitionOwnerAsync(message, partitionId, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Sends a message to the target owning a partition.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendToPartitionOwnerAsync(ClientMessage message, int partitionId, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (partitionId < 0) throw new ArgumentOutOfRangeException(nameof(partitionId));

            message.PartitionId = partitionId;

            var memberId = Partitioner.GetPartitionOwner(partitionId);
            var task = memberId == default
                ? SendAsync(message, cancellationToken)
                : SendToMemberAsync(message, memberId, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">An optional client.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        private
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendAsyncInternal(ClientMessage message, ClientConnection client, CancellationToken cancellationToken)
        {
            var task = SendAsyncInternal(message, client, _correlationIdSequence.GetNext(), cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">An optional client.</param>
        /// <param name="correlationId">A correlation identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        private async Task<ClientMessage> SendAsyncInternal(ClientMessage message, ClientConnection client, long correlationId, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // fail fast
            if (_disposed == 1 || _clusterState != ClusterState.Connected)
                throw new ClientNotConnectedException();

            // assign a unique identifier to the message
            // and send in one fragment, with proper flags
            message.CorrelationId = correlationId;
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // create the invocation
            // note that 'client' here may be null if the invocation is not bound to a client
            var invocation = new Invocation(message, _options.Messaging, client, cancellationToken);

            while (true)
            {
                try
                {
                    client ??= GetRandomClient(false);
                    if (client == null) throw new ClientNotConnectedException();
                    return await client.SendAsync(invocation, cancellationToken).CAF();
                }
                catch (Exception exception)
                {
                    // if the cluster is not connected anymore, die
                    if (_disposed == 1 || _clusterState != ClusterState.Connected)
                        throw new ClientNotConnectedException(exception);

                    // if it's retryable, and can be retried (no timeout etc), retry
                    // note that CanRetryAsync may wait (depending on the retry strategy)
                    if (invocation.ShouldRetry(exception, _options.Networking.RetryOnTargetDisconnected) &&
                        await invocation.CanRetryAsync(() => _correlationIdSequence.GetNext()).CAF())
                    {
                        HConsole.WriteLine(this, "Retrying...");
                        continue;
                    }

                    // else... it's bad enough
                    throw;
                }
            }
        }
    }
}
