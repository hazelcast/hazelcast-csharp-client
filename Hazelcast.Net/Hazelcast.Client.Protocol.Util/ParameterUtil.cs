// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol.Util
{
    internal sealed class ParameterUtil
    {
        private const int Utf8MaxBytesPerChar = 3;

        private ParameterUtil()
        {
        }

        public static int CalculateDataSize(string @string)
        {
            return Bits.IntSizeInBytes + @string.Length*Utf8MaxBytesPerChar;
        }

        public static int CalculateDataSize(IData data)
        {
            return CalculateDataSize(data.ToByteArray());
        }

        public static int CalculateDataSize(KeyValuePair<IData, IData> entry)
        {
            return CalculateDataSize(entry.Key.ToByteArray()) + CalculateDataSize(entry.Value.ToByteArray());
        }

        public static int CalculateDataSize(byte[] bytes)
        {
            return Bits.IntSizeInBytes + bytes.Length;
        }

        public static int CalculateDataSize(int data)
        {
            return Bits.IntSizeInBytes;
        }

        public static int CalculateDataSize(long data)
        {
            return Bits.LongSizeInBytes;
        }

        public static int CalculateDataSize(bool data)
        {
            return Bits.BooleanSizeInBytes;
        }
    }
}