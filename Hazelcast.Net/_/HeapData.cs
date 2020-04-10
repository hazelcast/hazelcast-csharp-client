using System;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast
{
    /// <summary>
    /// Implements <see cref="IData"/> on the heap.
    /// </summary>
    internal sealed class HeapData : IData
    {
        // structure is:
        // partition-hash (4 bytes) | type (4 bytes) | data (byte[])

        internal const int PartitionHashOffset = 0;
        internal const int TypeOffset = 4;
        internal const int DataOffset = 8;

        private const int HeapDataOverHead = DataOffset;

        private const int ArrayHeaderSizeInBytes = 16;

        private readonly byte[] _bytes;

        /// <summary>
        /// Initializes a new empty instance of the <see cref="HeapData"/> class.
        /// </summary>
        public HeapData()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeapData"/> class.
        /// </summary>
        /// <param name="bytes">The data bytes.</param>
        public HeapData(byte[] bytes)
        {
            //??
            // will use a byte to store partition_hash bit
            // array (12: array header, 4: length)

            // type and partition hash are always written with BIG_ENDIAN byte order
            // either we don't have bytes, or we have enough bytes
            if (bytes != null && bytes.Length > 0 && bytes.Length < HeapDataOverHead)
                throw new ArgumentException($"Data should either be empty or contain at least {HeapDataOverHead} bytes.");

            _bytes = bytes;
        }

        /// <inheritdoc />
        public int DataSize => Math.Max(TotalSize - HeapDataOverHead, 0);

        /// <inheritdoc />
        public int TotalSize => _bytes?.Length ?? 0;

        /// <inheritdoc />
        public int PartitionHash
        {
            get
            {
                int hash;
                return _bytes != null &&
                       _bytes.Length >= HeapDataOverHead &&
                       (hash = _bytes.ReadInt32(PartitionHashOffset, true)) != 0
                    ? hash
                    : GetHashCode();
            }
        }

        public bool HasPartitionHash
            => _bytes != null &&
               _bytes.Length >= HeapDataOverHead &&
               _bytes.ReadInt32(PartitionHashOffset, true) != 0;

        /// <inheritdoc />
        public byte[] ToByteArray() => _bytes ?? Array.Empty<byte>(); // FIXME should this be null?

        /// <inheritdoc />
        public int TypeId
            => TotalSize == 0 ? SerializationConstants.ConstantTypeNull : _bytes.ReadInt32(TypeOffset, true);

        /// <inheritdoc />
        public int HeapCost
            // where does this come from?
            => BytesExtensions.SizeOfInt32 + (_bytes != null ? ArrayHeaderSizeInBytes + _bytes.Length : 0);

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

            return dataSize == 0 || Equals(_bytes, data.ToByteArray());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Murmur3HashCode.Hash(_bytes, DataOffset, DataSize);
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

        // Same as Arrays.equals(byte[] a, byte[] a2) but loop order is reversed.
        private static bool Equals(byte[] data1, byte[] data2)
        {
            if (data1 == data2)
                return true;

            if (data1 == null || data2 == null)
                return false;

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
