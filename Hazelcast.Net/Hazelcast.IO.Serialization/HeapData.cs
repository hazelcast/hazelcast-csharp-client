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

using System;
using System.Linq;
using System.Text;
using Hazelcast.Util;

namespace Hazelcast.IO.Serialization
{
    internal sealed class HeapData : IData
    {
        internal const int PartitionHashOffset = 0;
        internal const int TypeOffset = 4;
        internal const int DataOffset = 8;

        private const int ArrayHeaderSizeInBytes = 16;

        //first 4 byte is type id + last 4 byte is partition hash code
        private const int HeapDataOverHead = DataOffset;

        private readonly ArraySegment<byte> _data;

        public HeapData(ArraySegment<byte> data)
        {
            // type and partition_hash are always written with BIG_ENDIAN byte-order
            // will use a byte to store partition_hash bit
            // array (12: array header, 4: length)
            if (data != null && data.Count > 0 && data.Count < HeapDataOverHead)
            {
                throw new ArgumentException("Data should be either empty or should contain more than " +
                                            HeapDataOverHead);
            }
            _data = data;
        }

        static byte[] Empty = new byte[0];

        public HeapData(byte[] data)
            : this(new ArraySegment<byte>(data ?? Empty))
        {
        }

        public int DataSize()
        {
            return Math.Max(TotalSize() - HeapDataOverHead, 0);
        }

        public int TotalSize()
        {
            return _data.Count;
        }

        public int GetPartitionHash()
        {
            if (HasPartitionHash())
            {
                return Bits.ReadIntB(_data.Array, _data.Offset + PartitionHashOffset);
            }
            return GetHashCode();
        }

        public bool HasPartitionHash()
        {
            return _data.Count >= HeapDataOverHead
                   && Bits.ReadIntB(_data.Array, _data.Offset + PartitionHashOffset) != 0;
        }

        public byte[] ToByteArray()
        {
            // deep copy here, soon to be removed
            var bytes = new byte[_data.Count];
            Buffer.BlockCopy(_data.Array,_data.Offset, bytes, 0, _data.Count);
            return bytes;
        }

        public ArraySegment<byte> ToByteArraySegment()
        {
            return _data;
        }

        public int GetTypeId()
        {
            if (TotalSize() == 0)
            {
                return SerializationConstants.ConstantTypeNull;
            }
            return Bits.ReadIntB(_data.Array, _data.Offset + TypeOffset);
        }

        public int GetHeapCost()
        {
            // reference (assuming compressed oops)
            var objectRef = Bits.IntSizeInBytes;
            return objectRef + (_data.Count > 0 ? ArrayHeaderSizeInBytes + _data.Count : 0);
        }

        public bool IsPortable()
        {
            return SerializationConstants.ConstantTypePortable == GetTypeId();
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (!(o is IData))
            {
                return false;
            }
            var data = (IData)o;
            if (GetTypeId() != data.GetTypeId())
            {
                return false;
            }
            var dataSize = DataSize();
            if (dataSize != data.DataSize())
            {
                return false;
            }
            return dataSize == 0 || _data.IsEqual(data.ToByteArraySegment());
        }

        public override int GetHashCode()
        {
            return HashUtil.MurmurHash3_x86_32(_data.Array, _data.Offset + DataOffset, DataSize());
        }

        public override string ToString()
        {
            var sb = new StringBuilder("DefaultData{");
            sb.Append("type=").Append(GetTypeId());
            sb.Append(", hashCode=").Append(GetHashCode());
            sb.Append(", partitionHash=").Append(GetPartitionHash());
            sb.Append(", totalSize=").Append(TotalSize());
            sb.Append(", dataSize=").Append(DataSize());
            sb.Append(", heapCost=").Append(GetHeapCost());
            sb.Append('}');
            return sb.ToString();
        }

        // Same as Arrays.equals(byte[] a, byte[] a2) but loop order is reversed.
        private static bool Equals(byte[] data1, byte[] data2)
        {
            if (data1 == data2)
            {
                return true;
            }
            if (data1 == null || data2 == null)
            {
                return false;
            }
            var length = data1.Length;
            if (data2.Length != length)
            {
                return false;
            }
            for (var i = length - 1; i >= DataOffset; i--)
            {
                if (data1[i] != data2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}