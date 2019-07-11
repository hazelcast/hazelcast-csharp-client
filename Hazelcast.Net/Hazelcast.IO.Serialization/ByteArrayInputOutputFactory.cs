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

using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class ByteArrayInputOutputFactory : IInputOutputFactory
    {
        private readonly ByteOrder _byteOrder;

        public ByteArrayInputOutputFactory(ByteOrder byteOrder)
        {
            _byteOrder = byteOrder;
        }

        public IBufferObjectDataInput CreateInput(IData data, ISerializationService service)
        {
            var s = data.ToByteArraySegment();
            return new ByteArrayObjectDataInput(s.Slice(HeapData.DataOffset), service, _byteOrder);
        }

        public IBufferObjectDataInput CreateInput(byte[] buffer, ISerializationService service)
        {
            return new ByteArrayObjectDataInput(buffer, service, _byteOrder);
        }

        public IBufferObjectDataOutput CreateOutput(int size, ISerializationService service)
        {
            return new ByteArrayObjectDataOutput(size, service, _byteOrder);
        }

        public ByteOrder GetByteOrder()
        {
            return _byteOrder;
        }
    }
}