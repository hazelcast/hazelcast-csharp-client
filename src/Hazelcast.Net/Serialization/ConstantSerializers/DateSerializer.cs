// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class DateSerializer : SingletonSerializerBase<DateTime>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override int TypeId => SerializationConstants.JavaDefaultTypeDate;

        public override DateTime Read(IObjectDataInput input)
        {
            return FromEpochTime(input.ReadLong());
        }

        public override void Write(IObjectDataOutput output, DateTime obj)
        {
            output.WriteLong(ToEpochDateTime(obj));
        }

        private static DateTime FromEpochTime(long sinceEpoxMillis)
        {
            return Epoch.AddMilliseconds(sinceEpoxMillis);
        }

        private static long ToEpochDateTime(DateTime dateTime)
        {
            return (long) dateTime.Subtract(Epoch).TotalMilliseconds;
        }
    }
}
