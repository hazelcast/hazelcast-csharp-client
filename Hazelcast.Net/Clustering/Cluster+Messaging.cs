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
        // TODO: add timeout support to all methods

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
        /// <param name="timeoutMilliseconds">The optional maximum number of milliseconds to get a response.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, Guid targetId, int timeoutMilliseconds = 0)
        {
            Client client;

            if (targetId == default)
            {
                client = GetRandomClient();
            }
            else
            {
                // TODO: original code falls back on random client
                if (!_clients.TryGetValue(targetId, out client))
                    throw new InvalidOperationException(ExceptionMessages.InvalidTarget);
            }

            if (client == null) throw new InvalidOperationException("Could not get a client.");

            return await client.SendAsync(message, _correlationIdSequence.Next, timeoutMilliseconds);
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
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, IData key)
        {
            var partitionId = Partitioner.GetPartitionId(key);
            return await SendAsync(message, partitionId);
        }

        /// <summary>
        /// Sends a message to the target owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        // TODO: consider removing that one, so that Partitioner does not require the serialization service
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, object key)
        {
            var partitionId = Partitioner.GetPartitionId(key);
            var targetId = Partitioner.GetPartitionOwner(partitionId);
            return await SendAsync(message, targetId);
        }

        /// <summary>
        /// Sends a message to the target owning a partition.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, int partitionId)
        {
            // TODO: all methods should test!
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (partitionId < 0) throw new ArgumentOutOfRangeException(nameof(partitionId));
            message.PartitionId = partitionId;
            var targetId = Partitioner.GetPartitionOwner(partitionId);
            return await SendAsync(message, targetId);
        }
    }
}
