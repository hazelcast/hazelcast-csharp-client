using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Serialization;

namespace Hazelcast.Clustering
{
    public partial class Cluster // Messaging
    {
        /// <summary>
        /// Sends a message to a random target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendAsync(ClientMessage message, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            var task = GetRandomClient()
                .SendAsync(message, _correlationIdSequence.Next, cancellation.Token)
                .ThenDispose(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.ConfigureAwait(false);
#endif
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
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendToMemberAsync(ClientMessage message, Guid memberId, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // try to get the specified member, else use a random member
            // connections to members are maintained elsewhere - we don't lazy-connect on demand
            if (memberId == default || !_clients.TryGetValue(memberId, out var client))
                client = GetRandomClient();

            if (client == null) throw new InvalidOperationException("Could not get a client.");

            var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            var task = client
                .SendAsync(message, _correlationIdSequence.Next, cancellation.Token)
                .ThenDispose(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">The target.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendToClientAsync(ClientMessage message, Client client, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (client == null) throw new ArgumentNullException(nameof(client));

            var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            var task = client
                .SendAsync(message, _correlationIdSequence.Next, true, cancellation.Token)
                .ThenDispose(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Sends a message to the target owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<ClientMessage> SendToKeyPartitionOwnerAsync(ClientMessage message, IData key, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (key == null) throw new ArgumentNullException(nameof(key));

            var partitionId = Partitioner.GetPartitionId(key);
            if (partitionId < 0) throw new ArgumentException("Could not get a partition for this key.", nameof(key));

            var task = SendToPartitionOwnerAsync(message, partitionId, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.ConfigureAwait(false);
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
#if !OPTIMIZE_ASYNC
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

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.ConfigureAwait(false);
#endif
        }
    }
}
