using System;
using System.Text;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    /// <summary>A Packet is a piece of data send over the line.</summary>
    internal sealed class Packet : ISocketWritable, ISocketReadable
    {
        public const byte Version = 4;
        public const int HeaderOp = 0;
        public const int HeaderResponse = 1;
        public const int HeaderEvent = 2;
        public const int HeaderWanReplication = 3;
        public const int HeaderUrgent = 4;
        public const int HeaderBind = 5;
        private const short PersistVersion = 1;
        private const short PersistHeader = 2;
        private const short PersistPartition = 3;
        private const short PersistSize = 4;
        private const short PersistValue = 5;
        private const short PersistCompleted = short.MaxValue;
        private IData data;
        private short header;
        private int partitionId;
        private short persistStatus;
        private int size;
        private int valueOffset;

        public Packet()
        {
        }

        public Packet(IData data)
            : this(data, -1)
        {
        }

        public Packet(IData data, int partitionId)
        {
            // The value of these constants is important. The order needs to match the order in the read/write process
            // These 2 fields are only used during read/write. Otherwise they have no meaning.
            // Stores the current 'phase' of read/write. This is needed so that repeated calls can be made to read/write.
            this.data = data;
            this.partitionId = partitionId;
        }

        public bool ReadFrom(ByteBuffer source)
        {
            if (!ReadVersion(source))
            {
                return false;
            }
            if (!ReadHeader(source))
            {
                return false;
            }
            if (!ReadPartition(source))
            {
                return false;
            }
            if (!ReadSize(source))
            {
                return false;
            }
            if (!ReadValue(source))
            {
                return false;
            }
            SetPersistStatus(PersistCompleted);
            return true;
        }

        public bool IsUrgent()
        {
            return IsHeaderSet(HeaderUrgent);
        }

        public bool WriteTo(ByteBuffer destination)
        {
            if (!WriteVersion(destination))
            {
                return false;
            }
            if (!WriteHeader(destination))
            {
                return false;
            }
            if (!WritePartition(destination))
            {
                return false;
            }
            if (!WriteSize(destination))
            {
                return false;
            }
            if (!WriteValue(destination))
            {
                return false;
            }
            SetPersistStatus(PersistCompleted);
            return true;
        }

        public void SetHeader(int bit)
        {
            header |= (short)(1 << bit);
        }

        public bool IsHeaderSet(int bit)
        {
            return (header & 1 << bit) != 0;
        }

        /// <summary>Returns the header of the Packet.</summary>
        /// <remarks>
        ///     Returns the header of the Packet. The header is used to figure out what the content is of this Packet before
        ///     the actual payload needs to be processed.
        /// </remarks>
        /// <returns>the header.</returns>
        public short GetHeader()
        {
            return header;
        }

        /// <summary>Returns the partition id of this packet.</summary>
        /// <remarks>Returns the partition id of this packet. If this packet is not for a particular partition, -1 is returned.</remarks>
        /// <returns>the partition id.</returns>
        public int GetPartitionId()
        {
            return partitionId;
        }

        // ========================= version =================================================
        private bool ReadVersion(ByteBuffer source)
        {
            if (!IsPersistStatusSet(PersistVersion))
            {
                if (!source.HasRemaining())
                {
                    return false;
                }
                var version = source.Get();
                SetPersistStatus(PersistVersion);
                if (Version != version)
                {
                    throw new ArgumentException("Packet versions are not matching! Expected -> " + Version +
                                                ", Incoming -> " + version);
                }
            }
            return true;
        }

        private bool WriteVersion(ByteBuffer destination)
        {
            if (!IsPersistStatusSet(PersistVersion))
            {
                if (!destination.HasRemaining())
                {
                    return false;
                }
                destination.Put(Version);
                SetPersistStatus(PersistVersion);
            }
            return true;
        }

        // ========================= header =================================================
        private bool ReadHeader(ByteBuffer source)
        {
            if (!IsPersistStatusSet(PersistHeader))
            {
                if (source.Remaining() < 2)
                {
                    return false;
                }
                header = source.GetShort();
                SetPersistStatus(PersistHeader);
            }
            return true;
        }

        private bool WriteHeader(ByteBuffer destination)
        {
            if (!IsPersistStatusSet(PersistHeader))
            {
                if (destination.Remaining() < Bits.ShortSizeInBytes)
                {
                    return false;
                }
                destination.PutShort(header);
                SetPersistStatus(PersistHeader);
            }
            return true;
        }

        // ========================= partition =================================================
        private bool ReadPartition(ByteBuffer source)
        {
            if (!IsPersistStatusSet(PersistPartition))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                partitionId = source.GetInt();
                SetPersistStatus(PersistPartition);
            }
            return true;
        }

        private bool WritePartition(ByteBuffer destination)
        {
            if (!IsPersistStatusSet(PersistPartition))
            {
                if (destination.Remaining() < Bits.IntSizeInBytes)
                {
                    return false;
                }
                destination.PutInt(partitionId);
                SetPersistStatus(PersistPartition);
            }
            return true;
        }

        // ========================= size =================================================
        private bool ReadSize(ByteBuffer source)
        {
            if (!IsPersistStatusSet(PersistSize))
            {
                if (source.Remaining() < Bits.IntSizeInBytes)
                {
                    return false;
                }
                size = source.GetInt();
                SetPersistStatus(PersistSize);
            }
            return true;
        }

        private bool WriteSize(ByteBuffer destination)
        {
            if (!IsPersistStatusSet(PersistSize))
            {
                if (destination.Remaining() < Bits.IntSizeInBytes)
                {
                    return false;
                }
                size = data.TotalSize();
                destination.PutInt(size);
                SetPersistStatus(PersistSize);
            }
            return true;
        }

        // ========================= value =================================================
        private bool WriteValue(ByteBuffer destination)
        {
            if (!IsPersistStatusSet(PersistValue))
            {
                if (size > 0)
                {
                    // the number of bytes that can be written to the bb.
                    var bytesWritable = destination.Remaining();
                    // the number of bytes that need to be written.
                    var bytesNeeded = size - valueOffset;
                    int bytesWrite;
                    bool done;
                    if (bytesWritable >= bytesNeeded)
                    {
                        // All bytes for the value are available.
                        bytesWrite = bytesNeeded;
                        done = true;
                    }
                    else
                    {
                        // Not all bytes for the value are available. So lets write as much as is available.
                        bytesWrite = bytesWritable;
                        done = false;
                    }
                    byte[] byteArray = data.ToByteArray();
                    destination.Put(byteArray, valueOffset, bytesWrite);
                    valueOffset += bytesWrite;
                    if (!done)
                    {
                        return false;
                    }
                }
                SetPersistStatus(PersistValue);
            }
            return true;
        }

        private bool ReadValue(ByteBuffer source)
        {
            if (!IsPersistStatusSet(PersistValue))
            {
                byte[] bytes;
                if (data == null)
                {
                    bytes = new byte[size];
                    data = new DefaultData(bytes);
                }
                else
                {
                    bytes = data.ToByteArray();
                }
                if (size > 0)
                {
                    var bytesReadable = source.Remaining();
                    var bytesNeeded = size - valueOffset;
                    bool done;
                    int bytesRead;
                    if (bytesReadable >= bytesNeeded)
                    {
                        bytesRead = bytesNeeded;
                        done = true;
                    }
                    else
                    {
                        bytesRead = bytesReadable;
                        done = false;
                    }
                    // read the data from the byte-buffer into the bytes-array.
                    source.Get(bytes, valueOffset, bytesRead);
                    valueOffset += bytesRead;
                    if (!done)
                    {
                        return false;
                    }
                }
                SetPersistStatus(PersistValue);
            }
            return true;
        }

        /// <summary>Returns an estimation of the packet, including its payload, in bytes.</summary>
        /// <returns>the size of the packet.</returns>
        public int Size()
        {
            // 11 = byte(version) + short(header) + int(partitionId) + int(data size)
            return (data != null ? data.TotalSize() : 0) + 11;
        }

        public IData GetData()
        {
            return data;
        }

        public void SetData(IData data)
        {
            this.data = data;
        }

        public bool Done()
        {
            return IsPersistStatusSet(PersistCompleted);
        }

        public void Reset()
        {
            data = null;
            persistStatus = 0;
        }

        private void SetPersistStatus(short persistStatus)
        {
            this.persistStatus = persistStatus;
        }

        private bool IsPersistStatusSet(short status)
        {
            return persistStatus >= status;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Packet{");
            sb.Append("header=").Append(header);
            sb.Append(", isResponse=").Append(IsHeaderSet(HeaderResponse));
            sb.Append(", isOperation=").Append(IsHeaderSet(HeaderOp));
            sb.Append(", isEvent=").Append(IsHeaderSet(HeaderEvent));
            sb.Append(", partitionId=").Append(partitionId);
            sb.Append('}');
            return sb.ToString();
        }
    }
}