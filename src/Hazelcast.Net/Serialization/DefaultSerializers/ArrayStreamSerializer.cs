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

namespace Hazelcast.Serialization.DefaultSerializers
{
    internal class ArrayStreamSerializer : ConstantSerializers.SingletonSerializerBase<object[]>
    {
        public override int GetTypeId() => SerializationConstants.JavaDefaultTypeArray;

        public override object[] Read(IObjectDataInput input)
        {
            var length = input.ReadInt();
            var objects = new object[length];
            for (var i = 0; i < length; i++)
            {
                objects[i] = input.ReadObject<object>();
            }
            return objects;
        }

        public override void Write(IObjectDataOutput output, object[] obj)
        {
            output.WriteInt(obj.Length);
            foreach (var t in obj)
            {
                output.WriteObject(t);
            }
        }
    }
}