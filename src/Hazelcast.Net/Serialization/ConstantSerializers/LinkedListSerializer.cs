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
using System.Collections.Generic;
using Hazelcast.Core;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class LinkedListSerializer<T> : SingletonSerializerBase<LinkedList<T>>
    {
        public override int TypeId => SerializationConstants.JavaDefaultTypeLinkedList;

        public override LinkedList<T> Read(IObjectDataInput input)
        {
            var size = input.ReadInt();
            if (size <= BytesExtensions.SizeOfNullArray) return null;

            var list = new LinkedList<T>();
            for (var i = 0; i < size; i++)
            {
                list.AddLast(input.ReadObject<T>());
            }
            return list;
        }

        public override void Write(IObjectDataOutput output, LinkedList<T> obj)
        {
            var size = obj?.Count ?? BytesExtensions.SizeOfNullArray;
            output.WriteInt(size);
            foreach (var o in obj)
            {
                output.WriteObject(o);
            }
        }
    }
}
