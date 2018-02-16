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

using System;

namespace Hazelcast.Net.Ext
{
    /// <summary>
    /// Represent the endienness 
    /// </summary>
    public class ByteOrder
    {
        public const string BigEndianText = "BIG_ENDIAN";
        public const string LittleEndianText = "LITTLE_ENDIAN";

        /// <summary>
        /// Big Endian
        /// </summary>
        public static readonly ByteOrder BigEndian = new ByteOrder("BIG_ENDIAN");

        /// <summary>
        /// Little endian
        /// </summary>
        public static readonly ByteOrder LittleEndian = new ByteOrder("LITTLE_ENDIAN");

        private string _name;

        private ByteOrder(string name)
        {
            _name = name;
        }

        public static ByteOrder GetByteOrder(string name)
        {
            return BigEndianText.Equals(name) ? BigEndian : LittleEndian;
        }

        public static ByteOrder NativeOrder()
        {
            return BitConverter.IsLittleEndian ? LittleEndian : BigEndian;
        }
    }
}