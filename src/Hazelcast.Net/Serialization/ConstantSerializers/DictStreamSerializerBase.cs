// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;

namespace Hazelcast.Serialization.ConstantSerializers
{
    // TODO: BigDecimal

    internal abstract class DictStreamSerializerBase<DType> : IStreamSerializer<DType>
            where DType : IDictionary<object, object>
        {
            public abstract int TypeId { get; }

            public virtual void Dispose()
            { }

            public abstract DType Read(IObjectDataInput input);

            public void Write(IObjectDataOutput output, DType obj)
            {
                var size = obj.Count;
                output.WriteInt(size);
                if (size > 0)
                {
                    foreach (var kvp in obj)
                    {
                        output.WriteObject(kvp.Key);
                        output.WriteObject(kvp.Value);
                    }
                }
            }

            protected static DType DeserializeEntries(IObjectDataInput input, int size, DType result)
            {
                for (int i = 0; i < size; i++)
                {
                    result.Add(input.ReadObject<object>(), input.ReadObject<object>());
                }
                return result;
            }
        }
}
