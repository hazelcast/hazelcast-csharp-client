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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientRingbufferProxy<T> : ClientProxy, IRingbuffer<T>
    {
        /// <summary>
        ///     The maximum number of items that can be retrieved in 1 go using the  <see cref="IRingbuffer{T}.ReadManyAsync" />
        ///     method.
        /// </summary>
        public const int MaxBatchSize = 1000;

        private long _capacity = -1;

        public ClientRingbufferProxy(string serviceName, string objectName, HazelcastClient client) : base(serviceName, objectName, client)
        {
        }

        public long Capacity()
        {
            if (_capacity != -1) return _capacity;

            var request = RingbufferCapacityCodec.EncodeRequest(Name);
            return _capacity = Invoke(request, m =>
                RingbufferCapacityCodec.DecodeResponse(m).Response);
        }

        public long Size()
        {
            var request = RingbufferSizeCodec.EncodeRequest(Name);
            return Invoke(request, m => RingbufferSizeCodec.DecodeResponse(m).Response);
        }

        public long TailSequence()
        {
            var request = RingbufferTailSequenceCodec.EncodeRequest(Name);
            return Invoke(request, m => RingbufferTailSequenceCodec.DecodeResponse(m).Response);
        }

        public long HeadSequence()
        {
            var request = RingbufferHeadSequenceCodec.EncodeRequest(Name);
            return Invoke(request, m => RingbufferHeadSequenceCodec.DecodeResponse(m).Response);
        }

        public long RemainingCapacity()
        {
            var request = RingbufferRemainingCapacityCodec.EncodeRequest(Name);
            return Invoke(request, m => RingbufferRemainingCapacityCodec.DecodeResponse(m).Response);
        }

        public long Add(T item)
        {
            ValidationUtil.ThrowExceptionIfNull(item, "Item cannot be null");

            var request = RingbufferAddCodec.EncodeRequest(Name, (int) OverflowPolicy.Overwrite, ToData(item));
            return Invoke(request, m => RingbufferAddCodec.DecodeResponse(m).Response);
        }

        public Task<long> AddAsync(T item, OverflowPolicy overflowPolicy)
        {
            ValidationUtil.ThrowExceptionIfNull(item, "Item cannot be null");

            var request = RingbufferAddCodec.EncodeRequest(Name, (int) OverflowPolicy.Overwrite, ToData(item));
            return InvokeAsync(request, GetPartitionKey(), m => RingbufferAddCodec.DecodeResponse(m).Response);
        }

        public T ReadOne(long sequence)
        {
            CheckSequence(sequence);

            var request = RingbufferReadOneCodec.EncodeRequest(Name, sequence);
            var response = Invoke(request, m => RingbufferReadOneCodec.DecodeResponse(m).Response);
            return ToObject<T>(response);
        }

        public Task<long> AddAllAsync<TE>(ICollection<TE> collection, OverflowPolicy overflowPolicy) where TE : T
        {
            ValidationUtil.ThrowExceptionIfTrue(collection.Count == 0, "Collection cannot be empty");

            var valueList = ToDataList(collection);
            var request = RingbufferAddAllCodec.EncodeRequest(Name, valueList, (int) overflowPolicy);
            return InvokeAsync(request, GetPartitionKey(), m => RingbufferAddAllCodec.DecodeResponse(m).Response);
        }

        public Task<IList<T>> ReadManyAsync(long startSequence, int minCount, int maxCount)
        {
            CheckSequence(startSequence);
            ValidationUtil.ThrowExceptionIfTrue(minCount < 0, "minCount can't be smaller than 0");
            ValidationUtil.ThrowExceptionIfTrue(maxCount < minCount, "maxCount should be equal or larger than minCount");
            ValidationUtil.ThrowExceptionIfTrue(minCount > Capacity(), "the minCount should be smaller than or equal to the capacity");
            ValidationUtil.ThrowExceptionIfTrue(maxCount > MaxBatchSize, "maxCount can't be larger than " + MaxBatchSize);

            var request = RingbufferReadManyCodec.EncodeRequest(Name, startSequence, minCount, maxCount, null);

            return InvokeAsync(request, GetPartitionKey(),
                m => ToList<T>(RingbufferReadManyCodec.DecodeResponse(m).Items));
        }

        protected override ClientMessage Invoke(ClientMessage request)
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