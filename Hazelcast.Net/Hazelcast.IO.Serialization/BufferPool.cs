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
using System.Threading;
using Hazelcast.Client.Spi;
using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
    internal class BufferPoolThreadLocal : IDisposable
    {
        /// <summary>
        /// Thread local has a finalizer and is properly disposable. Once the thread local is disposed, it removes itself from the buckets of the ThreadStatic field.
        /// </summary>
        private readonly ThreadLocal<BufferPool> _threadLocal;

        public BufferPoolThreadLocal(ISerializationService serializationService)
        {
            _threadLocal = new ThreadLocal<BufferPool>(() => new BufferPool(serializationService), false);
        }

        public BufferPool Get()
        {
            try
            {
                return _threadLocal.Value;
            }
            catch (ObjectDisposedException)
            {
                throw new HazelcastClientNotActiveException();
            }
        }

        public void Dispose()
        {
            _threadLocal.Dispose();
        }
    }

    internal class BufferPool
    {
        private const int MaxPooledItems = 3;

        private readonly ISerializationService _serializationService;

        // accessible for testing.
        readonly Queue<IBufferObjectDataOutput> outputQueue = new Queue<IBufferObjectDataOutput>(MaxPooledItems);

        readonly Queue<IBufferObjectDataInput> inputQueue = new Queue<IBufferObjectDataInput>(MaxPooledItems);

        public BufferPool(ISerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        public IBufferObjectDataOutput TakeOutputBuffer()
        {
            try
            {
                return outputQueue.Dequeue();
            }
            catch (InvalidOperationException)
            {
                return _serializationService.CreateObjectDataOutput();
            }
        }

        public void ReturnOutputBuffer(IBufferObjectDataOutput output)
        {
            if (output == null)
            {
                return;
            }
            output.Clear();
            OfferOrClose(outputQueue, output);
        }

        public IBufferObjectDataInput TakeInputBuffer(IData data)
        {
            IBufferObjectDataInput input;
            try
            {
                input = inputQueue.Dequeue();
            }
            catch (InvalidOperationException)
            {
                input = _serializationService.CreateObjectDataInput((byte[])null);
            }
            input.Init(data.ToByteArray(), HeapData.DataOffset);
            return input;
        }

        public void ReturnInputBuffer(IBufferObjectDataInput input)
        {
            if (input == null)
            {
                return;
            }
            input.Clear();
            OfferOrClose(inputQueue, input);
        }

        private static void OfferOrClose<C>(Queue<C> queue, C item) where C : IDisposable
        {
            if (queue.Count == MaxPooledItems)
            {
                IOUtil.CloseResource(item);
                return;
            }
            queue.Enqueue(item);
        }
    }
}