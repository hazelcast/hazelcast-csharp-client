// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    internal partial class SerializationService
    {
        // TODO: ObjectDataInput/Output still using copied buffers, not adjacent arrays
        // TODO: implement some pooling of some sort

        private ObjectDataOutput GetDataOutput()
            => _objectDataOutputPool.Get();

        private void ReturnDataOutput(ObjectDataOutput output)
            => _objectDataOutputPool.Return(output);

        private ObjectDataInput GetDataInput(IData data)
            => new ObjectDataInput(data.ToByteArray(), this, Endianness, HeapData.DataOffset);

        private static void ReturnDataInput(ObjectDataInput input)
            => input.Dispose();

        // TODO: methods below bypass any Get/Return pooling mechanisms we may create?

        // for tests
        public ObjectDataInput CreateObjectDataInput(byte[] data)
            => CreateObjectDataInput(data, Endianness);

        private ObjectDataInput CreateObjectDataInput(byte[] data, Endianness endianness)
            => new ObjectDataInput(data, this, endianness);

        // that one is used directly by portable
        public ObjectDataInput CreateObjectDataInput(IData data)
            => new ObjectDataInput(data.ToByteArray(), this, Endianness, HeapData.DataOffset);

        // for tests
        public ObjectDataOutput CreateObjectDataOutput()
            => GetDataOutput();

        // for tests
        public ObjectDataOutput CreateObjectDataOutput(int bufferSize)
        {
            var output = _objectDataOutputPool.Get();
            output.EnsureAvailable(bufferSize);
            return output;
        }
    }
}
