using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Implementation;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HRingBufferImplement
{
    /// <summary>
    /// Implements <see cref="IHRingBuffer{TItem}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    internal class HRingBuffer<TItem> : DistributedObjectBase, IHRingBuffer<TItem>
    {
        private readonly int _maxBatchSize;
        private long _capacity = -1;

        // FIXME why do some methods use the partition key and some dont?

        /// <summary>
        /// Initializes a new instance of the <see cref="HRingBuffer{TItem}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the ring buffer.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="maxBatchSize">The maximum read batch size.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HRingBuffer(string name, Cluster cluster, int maxBatchSize, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HRingBuffer.ServiceName, name, cluster, serializationService, loggerFactory)
        {
            _maxBatchSize = maxBatchSize;
        }

        /// <inheritdoc />
        public async Task<long> AddAsync(TItem item, CancellationToken cancellationToken)
            => await AddAsync(item, OverflowPolicy.Overwrite, cancellationToken).CAF();

        /// <inheritdoc />
        public async Task<long> AddAsync(TItem item, OverflowPolicy overflowPolicy, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = RingbufferAddCodec.EncodeRequest(Name, (int) overflowPolicy, itemData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return RingbufferAddCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> AddAsync<TAdding>(ICollection<TAdding> items, OverflowPolicy overflowPolicy, CancellationToken cancellationToken)
            where TAdding : TItem
        {
            if (items.Count == 0) throw new ArgumentException("Cannot add zero items.", nameof(items));
            var itemsData = ToSafeData(items);

            var requestMessage = RingbufferAddAllCodec.EncodeRequest(Name, itemsData, (int) overflowPolicy);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return RingbufferAddAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> GetCapacityAsync(CancellationToken cancellationToken)
        {
            if (_capacity != -1) return _capacity;

            var requestMessage = RingbufferCapacityCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return _capacity = RingbufferCapacityCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> GetHeadSequenceAsync(CancellationToken cancellationToken)
        {
            var requestMessage = RingbufferHeadSequenceCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return RingbufferHeadSequenceCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TItem>> ReadAsync(long startSequence, int minCount, int maxCount, CancellationToken cancellationToken)
        {
            if (startSequence < 0) throw new ArgumentOutOfRangeException(nameof(startSequence));
            if (minCount < 0) throw new ArgumentOutOfRangeException(nameof(minCount), "The value of minCount must be equal to, or greater than, zero.");
            if (maxCount < minCount) throw new ArgumentOutOfRangeException(nameof(maxCount), "The value of maxCount must be greater than, or equal to, the value of minCount.");

            var capacity = await GetCapacityAsync(cancellationToken).CAF();
            if (minCount > capacity) throw new ArgumentOutOfRangeException(nameof(minCount), "The value of minCount must be smaller than, or equal to, the capacity.");
            if (maxCount > _maxBatchSize) throw new ArgumentOutOfRangeException(nameof(maxCount), "The value of maxCount must be lower than, or equal to, the max batch size.");

            var requestMessage = RingbufferReadManyCodec.EncodeRequest(Name, startSequence, minCount, maxCount, null);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            var response = RingbufferReadManyCodec.DecodeResponse(responseMessage).Items;
            return new ReadOnlyLazyList<TItem>(response, SerializationService);
        }

        /// <inheritdoc />
        public async ValueTask<TItem> ReadAsync(long sequence, CancellationToken cancellationToken)
        {
            if (sequence < 0) throw new ArgumentOutOfRangeException(nameof(sequence));

            var requestMessage = RingbufferReadOneCodec.EncodeRequest(Name, sequence);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = RingbufferReadOneCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TItem>(response);
        }

        /// <inheritdoc />
        public async Task<long> GetRemainingCapacityAsync(CancellationToken cancellationToken)
        {
            var requestMessage = RingbufferRemainingCapacityCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return RingbufferRemainingCapacityCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = RingbufferSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return RingbufferSizeCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> GetTailSequenceAsync(CancellationToken cancellationToken)
        {
            var requestMessage = RingbufferTailSequenceCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return RingbufferTailSequenceCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
