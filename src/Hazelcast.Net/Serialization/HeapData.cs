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
using System.Collections.Generic;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Models;
namespace Hazelcast.Serialization
{
    /// <summary>
    /// Implements <see cref="IData"/> on the heap.
    /// </summary>
    /// <remarks>
    /// <para><strong>Memory layout</strong></para>
    /// <para>Every <see cref="HeapData"/> instance stores a contiguous byte region with the following layout:</para>
    /// <code>
    ///   [0..3]  partition-hash  (int32, big-endian)
    ///   [4..7]  type-id         (int32, big-endian)
    ///   [8..]   payload bytes
    /// </code>
    ///
    /// <para><strong>Memory ownership and buffer pooling</strong></para>
    /// <para>
    /// <see cref="HeapData"/> supports two ownership modes, determined at construction time and
    /// fixed for the lifetime of the instance:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <term>Non-pooled</term>
    ///     <description>
    ///     Created via <c>HeapData(byte[])</c> or <c>HeapData(Memory&lt;byte&gt;)</c> without a
    ///     <see cref="IBufferPool"/>. The backing array is plain GC-managed heap memory; <see cref="Dispose"/>
    ///     is a no-op and the GC reclaims the memory in the normal way. Instances decoded from incoming
    ///     network frames always fall into this category.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Pooled</term>
    ///     <description>
    ///     Created by <see cref="SerializationService"/> via <c>ObjectDataOutput.DetachBuffer()</c>,
    ///     which rents a buffer from an <see cref="IBufferPool"/>, serializes into it, then hands the
    ///     (content slice, backing array, pool) triple to
    ///     <c>HeapData(Memory&lt;byte&gt; content, byte[] backingBuffer, ..., IBufferPool)</c>.
    ///     Calling <see cref="Dispose"/> returns the backing array to the pool and clears
    ///     <see cref="MemoryBytes"/>. The instance must not be read after disposal.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <para><strong>Disposal and the frame-ownership model</strong></para>
    /// <para>
    /// When a pooled <see cref="HeapData"/> is encoded into a <c>ClientMessage</c> via
    /// <c>DataCodec.Encode</c>, the resulting <c>Frame</c> keeps a <c>Memory&lt;byte&gt;</c> view of
    /// the backing buffer and records the <see cref="HeapData"/> as its <em>owner</em>. Once the message
    /// has been transmitted, <c>ClientMessage.Dispose()</c> disposes every frame, which in turn calls
    /// <see cref="Dispose"/> on the owner, returning the pooled buffer.
    /// </para>
    /// <para>
    /// This means that <strong>a pooled <see cref="HeapData"/> must not be stored anywhere that
    /// outlives the request send operation</strong> — for example, as a dictionary key or in a cache.
    /// Code that needs to retain a key after sending must create a stable, non-pooled snapshot first:
    /// </para>
    /// <code>
    ///   using var pooledKey = serializationService.ToData(userKey);  // pooled
    ///   IData stableKey = new HeapData(pooledKey.ToByteArray());     // non-pooled copy
    ///   // ... use stableKey as dictionary key or NearCache key ...
    ///   // pooledKey is returned to the pool at the end of the using block
    /// </code>
    ///
    /// <para><strong><see cref="DeAttach"/> — transfer ownership without copying</strong></para>
    /// <para>
    /// <see cref="DeAttach"/> creates a fresh non-pooled <see cref="HeapData"/> containing the same
    /// bytes, clears <see cref="MemoryBytes"/> on the original, and returns the backing buffer to the
    /// pool. After the call the original instance is empty and must not be used; the returned instance
    /// is independently owned and safe to store long-term. This avoids copying when a single code path
    /// both sends the data and needs a long-lived copy.
    /// </para>
    ///
    /// <para><strong>Equality and hashing</strong></para>
    /// <para>
    /// <see cref="GetHashCode"/> and <see cref="Equals(object)"/> are based solely on the payload
    /// bytes (offset 8 onward). Because a disposed pooled instance has an empty <see cref="MemoryBytes"/>,
    /// its hash becomes 0 and equality comparisons will fail. Never use a disposed instance as a
    /// dictionary key.
    /// </para>
    /// </remarks>
    internal sealed class HeapData : IData, ICanHaveSchemas
    {
        // structure is:
        // partition-hash (4 bytes) | type (4 bytes) | data (byte[])

        internal const int PartitionHashOffset = 0;
        internal const int TypeOffset = 4;
        internal const int DataOffset = 8;

        private const int HeapDataOverHead = DataOffset;

        private const int ArrayHeaderSizeInBytes = 16;

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
        public HeapData(Memory<byte> memoryBytes,  HashSet<long> schemaIds = null)
        {
            if (memoryBytes.Length > 0 && memoryBytes.Length < HeapDataOverHead)
                throw new ArgumentException($"Data should either be empty or contain at least {HeapDataOverHead} bytes.");

            _bytes = Array.Empty<byte>();
             MemoryBytes = memoryBytes;
            _schemaIds = schemaIds;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HeapData"/> with a pre-sliced memory view and its
        /// backing buffer. The backing buffer will be returned to the pool on <see cref="Dispose"/>.
        /// </summary>
        /// <param name="content">The pre-sliced, correctly-bounded view of the written bytes.</param>
        /// <param name="backingBuffer">The raw rented array that backs <paramref name="content"/>, for pool return.</param>
        /// <param name="schemaIds">Schema identifiers.</param>
        /// <param name="bufferPool">The buffer pool to return <paramref name="backingBuffer"/> to on dispose.</param>
        public HeapData(Memory<byte> content, byte[] backingBuffer, HashSet<long> schemaIds = null, IBufferPool bufferPool = null)
        {
            if (content.Length > 0 && content.Length < HeapDataOverHead)
                throw new ArgumentException($"Data should either be empty or contain at least {HeapDataOverHead} bytes.");

            _bytes = backingBuffer;
            MemoryBytes = content;
            _bufferPool = bufferPool;
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
            return dataSize == 0 || Equals(MemoryBytes.Span, data.GetMemory().Span);
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
            // Only release resources when this instance manages a pooled buffer.
            // Non-pooled instances have no resources to release; the GC handles them.
            if (_bufferPool == null) return;

            _bufferPool.Return(_bytes);
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
