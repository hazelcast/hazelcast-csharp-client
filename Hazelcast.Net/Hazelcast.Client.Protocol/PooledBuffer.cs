// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Protocol.Util;

namespace Hazelcast.Client.Protocol
{
    struct PooledBuffer
    {
#if !NET40
        byte[] _toReturn;

        public PooledBuffer(byte[] memoryToReturn)
        {
            _toReturn = memoryToReturn;
        }
#endif

        public static PooledBuffer Alloc(int size, out IClientProtocolBuffer buffer)
        {
            byte[] bytes;
            var pooled = Alloc(size, out bytes);
            buffer = new SafeBuffer(bytes);
            return pooled;
        }

        public static PooledBuffer Alloc(int size, out byte[] buffer)
        {
#if !NET40
            buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(size);
            return new PooledBuffer(buffer);
#else
            buffer = new byte[size];
            return default(PooledBuffer);
#endif
        }

        public void Return()
        {
#if !NET40
            System.Buffers.ArrayPool<byte>.Shared.Return(_toReturn, true);
#endif
        }
    }
}