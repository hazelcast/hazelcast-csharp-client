using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientRingbufferProxy<E> : ClientProxy, IRingbuffer<E>
    {
        /// <summary>
        ///     The maximum number of items that can be retrieved in 1 go using the  <see cref="Ringbuffer{E}.ReadManyAsync" />
        ///     method.
        /// </summary>
        public const int MaxBatchSize = 1000;

        private long _capacity = -1;

        public ClientRingbufferProxy(string serviceName, string objectName) : base(serviceName, objectName)
        {
        }

        public long Capacity()
        {
            if (_capacity != -1) return _capacity;

            var request = RingbufferCapacityCodec.EncodeRequest(GetName());
            return _capacity = Invoke(request, m =>
                RingbufferCapacityCodec.DecodeResponse(m).response);
        }

        public long Size()
        {
            var request = RingbufferSizeCodec.EncodeRequest(GetName());
            return Invoke(request, m => RingbufferSizeCodec.DecodeResponse(m).response);
        }

        public long TailSequence()
        {
            var request = RingbufferTailSequenceCodec.EncodeRequest(GetName());
            return Invoke(request, m => RingbufferTailSequenceCodec.DecodeResponse(m).response);
        }

        public long HeadSequence()
        {
            var request = RingbufferHeadSequenceCodec.EncodeRequest(GetName());
            return Invoke(request, m => RingbufferHeadSequenceCodec.DecodeResponse(m).response);
        }

        public long RemainingCapacity()
        {
            var request = RingbufferRemainingCapacityCodec.EncodeRequest(GetName());
            return Invoke(request, m => RingbufferRemainingCapacityCodec.DecodeResponse(m).response);
        }

        public long Add(E item)
        {
            ThrowExceptionIfNull(item, "Item cannot be null");

            var request = RingbufferAddCodec.EncodeRequest(GetName(), (int) OverflowPolicy.Overwrite, ToData(item));
            return Invoke(request, m => RingbufferAddCodec.DecodeResponse(m).response);
        }

        public Task<long> AddAsync(E item, OverflowPolicy overflowPolicy)
        {
            ThrowExceptionIfNull(item, "Item cannot be null");

            var request = RingbufferAddAsyncCodec.EncodeRequest(GetName(), (int) OverflowPolicy.Overwrite, ToData(item));
            return InvokeAsync(request, GetPartitionKey(), m => RingbufferAddAsyncCodec.DecodeResponse(m).response);
        }

        public E ReadOne(long sequence)
        {
            CheckSequence(sequence);

            var request = RingbufferReadOneCodec.EncodeRequest(GetName(), sequence);
            var response = Invoke(request, m => RingbufferReadOneCodec.DecodeResponse(m).response);
            return ToObject<E>(response);
        }

        public Task<long> AddAllAsync<T>(ICollection<T> collection, OverflowPolicy overflowPolicy) where T : E
        {
            ThrowExceptionIfTrue(collection.Count == 0, "Collection cannot be empty");

            var valueList = ToDataList(collection);
            var request = RingbufferAddAllAsyncCodec.EncodeRequest(GetName(), valueList, (int) overflowPolicy);
            return InvokeAsync(request, GetPartitionKey(), m => RingbufferAddAllAsyncCodec.DecodeResponse(m).response);
        }

        public Task<IList<E>> ReadManyAsync(long startSequence, int minCount, int maxCount)
        {
            CheckSequence(startSequence);
            ThrowExceptionIfTrue(minCount < 0, "minCount can't be smaller than 0");
            ThrowExceptionIfTrue(maxCount < minCount, "maxCount should be equal or larger than minCount");
            ThrowExceptionIfTrue(minCount > Capacity(), "the minCount should be smaller than or equal to the capacity");
            ThrowExceptionIfTrue(maxCount > MaxBatchSize, "maxCount can't be larger than " + MaxBatchSize);

            var request = RingbufferReadManyAsyncCodec.EncodeRequest(GetName(), startSequence, minCount, maxCount, null);

            return InvokeAsync(request, GetPartitionKey(),
                m => ToList<E>(RingbufferReadManyAsyncCodec.DecodeResponse(m).items));
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetPartitionKey());
        }

        private void CheckSequence(long sequence)
        {
            if (sequence < 0)
            {
                throw new ArgumentException("Sequence can't be smaller than 0, but was: " + sequence);
            }
        }
    }
}