﻿using System;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Serialization;

namespace Hazelcast.Clustering
{
    // partial: messaging
    public partial class Cluster
    {
        private TimeoutCancellationTokenSource CreateTimeoutCancellationTokenSource(int timeoutSeconds)
        {
            var timeout = timeoutSeconds * 1000; // FIXME: why are we not getting a timespan? + other methods
            if (timeout == 0) timeout = Constants.Invocation.DefaultTimeoutSeconds;
            return _clusterCancellation.WithTimeout(timeout);
        }

        /// <summary>
        /// Sends a message to a random target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="timeoutSeconds">The optional maximum number of seconds to get a response.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
#if OPTIMIZE_ASYNC
        public ValueTask<ClientMessage> SendAsync(ClientMessage message, int timeoutSeconds = 0)
#else
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, int timeoutSeconds = 0)
#endif
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var cancellation = CreateTimeoutCancellationTokenSource(timeoutSeconds);

#if OPTIMIZE_ASYNC
            return
#else
            return await
#endif
                    GetRandomClient()
                        .SendAsync(message, _correlationIdSequence.Next, cancellation.Token)
                        .OrTimeout(cancellation)
#if !OPTIMIZE_ASYNC
                        .ConfigureAwait(false)
#endif
                ;
        }

        /// <summary>
        /// Sends a message to a member.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="timeoutSeconds">The optional maximum number of seconds to get a response.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        /// <remarks>
        /// <para>If <paramref name="memberId"/> is the default value, sends the message to a random member. If it
        /// is an unknown member, sends the message to a random number too.</para>
        /// </remarks>
#if OPTIMIZE_ASYNC
        public ValueTask<ClientMessage> SendToMemberAsync(ClientMessage message, Guid memberId, int timeoutSeconds = 0)
#else
        public async ValueTask<ClientMessage> SendToMemberAsync(ClientMessage message, Guid memberId, int timeoutSeconds = 0)
#endif
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // try to get the specified member, else use a random member
            // connections to members are maintained elsewhere - we don't lazy-connect on demand
            if (memberId == default || !_clients.TryGetValue(memberId, out var client))
                client = GetRandomClient();

            if (client == null) throw new InvalidOperationException("Could not get a client.");

            var cancellation = CreateTimeoutCancellationTokenSource(timeoutSeconds);

#if OPTIMIZE_ASYNC
            return
#else
            return await
#endif
                    client
                        .SendAsync(message, _correlationIdSequence.Next, cancellation.Token)
                        .OrTimeout(cancellation)
#if !OPTIMIZE_ASYNC
                        .ConfigureAwait(false)
#endif
                ;
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">The target.</param>
        /// <param name="timeoutSeconds">The optional maximum number of seconds to get a response.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
#if OPTIMIZE_ASYNC
        public ValueTask<ClientMessage> SendToClientAsync(ClientMessage message, Client client, int timeoutSeconds = 0)
#else
        public async ValueTask<ClientMessage> SendToClientAsync(ClientMessage message, Client client, int timeoutSeconds = 0)
#endif
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (client == null) throw new ArgumentNullException(nameof(client));

            var cancellation = CreateTimeoutCancellationTokenSource(timeoutSeconds);

#if OPTIMIZE_ASYNC
            return
#else
            return await
#endif
                    client
                        .SendAsync(message, _correlationIdSequence.Next, true, cancellation.Token)
                        .OrTimeout(cancellation)
#if !OPTIMIZE_ASYNC
                        .ConfigureAwait(false)
#endif
                ;
        }

        /// <summary>
        /// Sends a message to the target owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <param name="timeoutSeconds">The optional maximum number of seconds to get a response.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
#if OPTIMIZE_ASYNC
        public ValueTask<ClientMessage> SendToKeyPartitionOwnerAsync(ClientMessage message, IData key, int timeoutSeconds = 0)
#else
        public async ValueTask<ClientMessage> SendToKeyPartitionOwnerAsync(ClientMessage message, IData key, int timeoutSeconds = 0)
#endif
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (key == null) throw new ArgumentNullException(nameof(key));

            var partitionId = Partitioner.GetPartitionId(key);
            if (partitionId < 0) throw new ArgumentException("Could not get a partition for this key.", nameof(key));

#if OPTIMIZE_ASYNC
            return
#else
            return await
#endif
                    SendToPartitionOwnerAsync(message, partitionId, timeoutSeconds)
#if !OPTIMIZE_ASYNC
                        .ConfigureAwait(false)
#endif
                ;
        }

        /// <summary>
        /// Sends a message to the target owning a partition.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <param name="timeoutSeconds">The optional maximum number of seconds to get a response.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
#if OPTIMIZE_ASYNC
        public ValueTask<ClientMessage> SendToPartitionOwnerAsync(ClientMessage message, int partitionId, int timeoutSeconds = 0)
#else
        public async ValueTask<ClientMessage> SendToPartitionOwnerAsync(ClientMessage message, int partitionId, int timeoutSeconds = 0)
#endif
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (partitionId < 0) throw new ArgumentOutOfRangeException(nameof(partitionId));

            message.PartitionId = partitionId;

            var memberId = Partitioner.GetPartitionOwner(partitionId);

#if OPTIMIZE_ASYNC
            return
#else
            return await
#endif
                    (memberId == default
                        ? SendAsync(message, timeoutSeconds)
                        : SendToMemberAsync(message, memberId, timeoutSeconds))
#if !OPTIMIZE_ASYNC
                    .ConfigureAwait(false)
#endif
                ;
        }
    }
}
