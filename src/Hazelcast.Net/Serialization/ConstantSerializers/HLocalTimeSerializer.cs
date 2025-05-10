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
using Hazelcast.Models;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class HLocalTimeSerializer : SingletonSerializerBase<HLocalTime>
    {
        public override int TypeId => SerializationConstants.JavaDefaultTypeLocalTime;

        public override HLocalTime Read(IObjectDataInput input)
        {
            var hour = input.ReadByte();
            var minute = input.ReadByte();
            var second = input.ReadByte();
            var nano = input.ReadInt();

            return new HLocalTime(hour, minute, second, nano);
        }

        public override void Write(IObjectDataOutput output, HLocalTime obj)
        {
            output.WriteByte(obj.Hour);
            output.WriteByte(obj.Minute);
            output.WriteByte(obj.Second);
            output.WriteInt(obj.Nanosecond);
        }
    }
}
