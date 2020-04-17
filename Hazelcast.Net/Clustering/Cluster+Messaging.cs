using System;
using System.Threading.Tasks;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Serialization;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents an Hazelcast Cluster.
    /// </summary>
    public partial class Cluster // Messaging
    {
        /// <summary>
        /// Sends a message to a random target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message)
        {
            return await GetRandomClient().SendAsync(message, _correlationIdSequence.Next);
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="targetId">The identifier of the target.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, Guid targetId)
        {
            if (!_clients.TryGetValue(targetId, out var client))
                throw new InvalidOperationException(ExceptionMessages.InvalidTarget);
            return await client.SendAsync(message, _correlationIdSequence.Next);
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">The target.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, Client client)
            => await client.SendAsync(message, _correlationIdSequence.Next);

        /// <summary>
        /// Sends a message to the target owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendToKeyOwner(ClientMessage message, IData key)
        {
            var targetId = _partitioner.GetPartitionOwner(key);
            if (targetId == default)
                throw new InvalidOperationException(ExceptionMessages.InvalidTarget);
            return await SendAsync(message, targetId);
        }

        /// <summary>
        /// Sends a message to the target owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        // TODO: consider removing that one, so that Partitioner does not require the serialization service
        public async ValueTask<ClientMessage> SendToKeyOwner(ClientMessage message, object key)
        {
            var partitionId = _partitioner.GetPartitionId(key);
            var targetId = _partitioner.GetPartitionOwner(partitionId);
            if (targetId == default)
                throw new InvalidOperationException(ExceptionMessages.InvalidTarget);
            return await SendAsync(message, targetId);
        }

        /// <summary>
        /// Sends a message to the target owning a partition.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendToPartitionOwner(ClientMessage message, int partitionId)
        {
            var targetId = _partitioner.GetPartitionOwner(partitionId);
            if (targetId == default)
                throw new InvalidOperationException(ExceptionMessages.InvalidTarget);
            return await SendAsync(message, targetId);
        }
    }
}
