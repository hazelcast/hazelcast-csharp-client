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
using Hazelcast.Models;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class HOffsetDateTimeSerializer : SingletonSerializerBase<HOffsetDateTime>
    {
        public override int TypeId => SerializationConstants.JavaDefaultTypeOffsetDateTime;

        public override HOffsetDateTime Read(IObjectDataInput input)
        {
            var year = input.ReadInt();
            var month = input.ReadByte();
            var day = input.ReadByte();

            var hour = input.ReadByte();
            var minute = input.ReadByte();
            var second = input.ReadByte();
            var nano = input.ReadInt();

            var offsetSeconds = input.ReadInt();

            return new HOffsetDateTime(
                new HLocalDateTime(year, month, day, hour, minute, second, nano),
                TimeSpan.FromSeconds(offsetSeconds)
            );
        }

        public override void Write(IObjectDataOutput output, HOffsetDateTime obj)
        {
            var localDateTime = obj.LocalDateTime;

            output.WriteInt(localDateTime.Year);
            output.WriteByte(localDateTime.Month);
            output.WriteByte(localDateTime.Day);

            output.WriteByte(localDateTime.Hour);
            output.WriteByte(localDateTime.Minute);
            output.WriteByte(localDateTime.Second);
            output.WriteInt(localDateTime.Nanosecond);

            output.WriteInt((int)obj.Offset.TotalSeconds);
        }
    }
}
