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

using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    internal class BufferPool
    {
        private const int MaxPooledItems = 3;

        private readonly ObjectPool<IBufferObjectDataInput> _inputPool;
        private readonly ObjectPool<IBufferObjectDataOutput> _outputPool;

        public BufferPool(ISerializationService serializationService)
        {
            var serializationService1 = serializationService;

            _inputPool = new ObjectPool<IBufferObjectDataInput>(MaxPooledItems,
                () => serializationService1.CreateObjectDataInput((byte[])null),
                item => item.Clear());

            _outputPool = new ObjectPool<IBufferObjectDataOutput>(MaxPooledItems,
                () => serializationService1.CreateObjectDataOutput(),
                item => item.Clear());
        }

        public IBufferObjectDataOutput TakeOutputBuffer() => _outputPool.Take();

        public void ReturnOutputBuffer(IBufferObjectDataOutput output) => _outputPool.Return(output);

        public IBufferObjectDataInput TakeInputBuffer(IData data)
        {
            var input = _inputPool.Take();
            input.Init(data.ToByteArray(), HeapData.DataOffset);
            return input;
        }

        public void ReturnInputBuffer(IBufferObjectDataInput input) => _inputPool.Return(input);
    }
}