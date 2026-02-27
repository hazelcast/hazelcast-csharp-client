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
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Implements <see cref="IData"/> on the heap.
    /// </summary>
    internal sealed class HeapData : IData, ICanHaveSchemas
    {
        // structure is:
        // partition-hash (4 bytes) | type (4 bytes) | data (byte[])

        internal const int PartitionHashOffset = 0;
        internal const int TypeOffset = 4;
        internal const int DataOffset = 8;

        private const int HeapDataOverHead = DataOffset;

        private const int ArrayHeaderSizeInBytes = 16;

        //private readonly byte[] _bytes;
        private HashSet<long> _schemaIds;

        private byte[] _bytes;
        private bool _detached;
        private readonly IBufferPool _bufferPool;

        /// <summary>
        /// Initializes a new empty instance of the <see cref="HeapData"/> class.
        /// </summary>
        public HeapData()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeapData"/> class.
        /// </summary>
        /// <param name="bytes">The data bytes.</param>
        /// <param name="bufferPool">The buffer pool</param>
        /// <param name="schemaIds">Schema identifiers.</param>
        public HeapData(byte[] bytes, HashSet<long> schemaIds = null, IBufferPool bufferPool = null)
        {
            //??
            // will use a byte to store partition_hash bit
            // array (12: array header, 4: length)

            // type and partition hash are always written with BIG_ENDIAN byte order
            // either we don't have bytes, or we have enough bytes
            if (bytes != null && bytes.Length > 0 && bytes.Length < HeapDataOverHead)
                throw new ArgumentException($"Data should either be empty or contain at least {HeapDataOverHead} bytes.");

            _bufferPool = bufferPool; // null allowed, it means that the data is not pooled and will not be returned to any pool when disposed

            _bytes = bytes;
            MemoryBytes = new ReadOnlyMemory<byte>(_bytes);
            _schemaIds = schemaIds;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="HeapData"/> class.
        /// </summary>
        /// <param name="memoryBytes">Memory bytes</param>
        /// <param name="schemaIds">Schema Ids</param>
        /// <exception cref="ArgumentException"></exception>
        public HeapData(ReadOnlyMemory<byte> memoryBytes, HashSet<long> schemaIds = null)
        {
            if (memoryBytes.Length > 0 && memoryBytes.Length < HeapDataOverHead)
                throw new ArgumentException($"Data should either be empty or contain at least {HeapDataOverHead} bytes.");

            _bytes = Array.Empty<byte>();
             MemoryBytes = memoryBytes;
            _schemaIds = schemaIds;
        }

        public ReadOnlyMemory<byte> MemoryBytes { get; private set; } = Memory<byte>.Empty;

        /// <inheritdoc />
        public bool HasSchemas => _schemaIds != null;

        /// <inheritdoc />
        public HashSet<long> SchemaIds => _schemaIds ??= new HashSet<long>();

        /// <inheritdoc />
        public int DataSize => Math.Max(TotalSize - HeapDataOverHead, 0);

        /// <inheritdoc />
        public int TotalSize => MemoryBytes.Length;

        /// <inheritdoc />
        public int PartitionHash
        {
            get
            {
                int hash;
                return
                    MemoryBytes.Length >= HeapDataOverHead &&
                    (hash = MemoryBytes.ReadInt(PartitionHashOffset, Endianness.BigEndian)) != 0
                        ? hash
                        : GetHashCode();
            }
        }

        public bool HasPartitionHash
            => MemoryBytes.Length >= HeapDataOverHead &&
               MemoryBytes.ReadInt(PartitionHashOffset, Endianness.BigEndian) != 0;

        /// <inheritdoc />
        public byte[] ToByteArray() => MemoryBytes.ToArray();
        
        /// <inheritdoc />
        public ReadOnlyMemory<byte> GetMemory() => MemoryBytes;

        /// <inheritdoc />
        public int TypeId
            => TotalSize == 0 ? SerializationConstants.ConstantTypeNull : MemoryBytes.ReadInt(TypeOffset, Endianness.BigEndian);

        /// <inheritdoc />
        public int HeapCost
            // where does this come from?
            => BytesExtensions.SizeOfInt + (MemoryBytes.Length > 0 ? ArrayHeaderSizeInBytes + MemoryBytes.Length : 0);

        /// <inheritdoc />
        public bool IsPortable
            => SerializationConstants.ConstantTypePortable == TypeId;

        /// <inheritdoc />
        public override bool Equals(object o)
        {
            if (this == o)
                return true;

            if (!(o is IData data))
                return false;

            if (TypeId != data.TypeId)
                return false;

            var dataSize = DataSize;
            if (dataSize != data.DataSize)
                return false;

            // todo: optimize this by comparing partition hashes first, if they are present. Also, get rid of toByteArray() call by comparing MemoryBytes directly.
            return dataSize == 0 || Equals(MemoryBytes, data.GetMemory());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (DataSize == 0) return 0;
            return Murmur3HashCode.Hash(MemoryBytes.Span, DataOffset, DataSize);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder("DefaultData{");
            sb.Append("type=").Append(TypeId);
            sb.Append(", hashCode=").Append(GetHashCode());
            sb.Append(", partitionHash=").Append(PartitionHash);
            sb.Append(", totalSize=").Append(TotalSize);
            sb.Append(", dataSize=").Append(DataSize);
            sb.Append(", heapCost=").Append(HeapCost);
            sb.Append('}');
            return sb.ToString();
        }
        public void Dispose()
        {
            _bufferPool?.Return(_bytes);
            _bytes = null;
            MemoryBytes = ReadOnlyMemory<byte>.Empty;
            _schemaIds = null;
        }

        public IData DeAttach()
        {
            if (_detached) return this;

            var copy = new byte[MemoryBytes.Length];

            MemoryBytes.CopyTo(copy);
            MemoryBytes = ReadOnlyMemory<byte>.Empty;
            _bufferPool?.Return(_bytes);
            _bytes = null;
            _detached = true;

            return new HeapData(copy, _schemaIds);
        }


        // Same as Arrays.equals(byte[] a, byte[] a2) but loop order is reversed.
        private static bool Equals(ReadOnlySpan<byte> data1, ReadOnlySpan<byte> data2)
        {
            if (data1.SequenceEqual(data2))
                return true;

            var length = data1.Length;
            if (data2.Length != length)
                return false;

            unchecked
            {
                for (var i = length - 1; i >= DataOffset; i--)
                {
                    if (data1[i] != data2[i])
                        return false;
                }
            }

            return true;
        }

    }
}
