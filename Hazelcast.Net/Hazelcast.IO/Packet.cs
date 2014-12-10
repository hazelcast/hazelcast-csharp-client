using System;
using System.Text;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    /// <summary>A Packet is a piece of data send over the line.</summary>
    internal sealed class Packet : DataAdapter
    {
        public const byte Version = 3;

        public const int HeaderOp = 0;
        public const int HeaderResponse = 1;
        public const int HeaderEvent = 2;
        public const int HeaderWanReplication = 3;
        public const int HeaderUrgent = 4;
        public const int HeaderBind = 5;
        private const int StVersion = 10;
        private const int StHeader = 11;
        private const int StPartition = 12;

        private short header;
        private int partitionId;

        public Packet(IPortableContext context)
            : base(context)
        {
        }

        public Packet(IData value, IPortableContext context)
            : this(value, -1, context)
        {
        }

        public Packet(IData value, int partitionId, IPortableContext context)
            : base(value, context)
        {
            this.partitionId = partitionId;
        }

        public void SetHeader(int bit)
        {
            header |= (short) (1 << bit);
        }

        public bool IsHeaderSet(int bit)
        {
            return (header & 1 << bit) != 0;
        }

        /// <summary>Returns the header of the Packet.</summary>
        /// <remarks>
        /// Returns the header of the Packet. The header is used to figure out what the content is of this Packet before
        /// the actual payload needs to be processed.
        /// </remarks>
        /// <returns>the header.</returns>
        public short GetHeader()
        {
            return header;
        }

        /// <summary>Returns the partition id of this packet.</summary>
        /// <remarks>Returns the partition id of this packet. If this packet is not for a particular partition, -1 is returned.
        /// 	</remarks>
        /// <returns>the partition id.</returns>
        public int GetPartitionId()
        {
            return partitionId;
        }

        public override bool IsUrgent()
        {
            return IsHeaderSet(HeaderUrgent);
        }

        public override bool WriteTo(ByteBuffer destination)
        {
            if (!IsStatusSet(StVersion))
            {
                if (!destination.HasRemaining())
                {
                    return false;
                }
                destination.Put(Version);
                SetStatus(StVersion);
            }
            if (!IsStatusSet(StHeader))
            {
                if (destination.Remaining() < Bits.SHORT_SIZE_IN_BYTES)
                {
                    return false;
                }
                destination.PutShort(header);
                SetStatus(StHeader);
            }
            if (!IsStatusSet(StPartition))
            {
                if (destination.Remaining() < Bits.INT_SIZE_IN_BYTES)
                {
                    return false;
                }
                destination.PutInt(partitionId);
                SetStatus(StPartition);
            }
            return base.WriteTo(destination);
        }

        public override bool ReadFrom(ByteBuffer source)
        {
            if (!IsStatusSet(StVersion))
            {
                if (!source.HasRemaining())
                {
                    return false;
                }
                byte version = source.Get();
                SetStatus(StVersion);
                if (Version != version)
                {
                    throw new ArgumentException("Packet versions are not matching! This -> " + Version
                         + ", Incoming -> " + version);
                }
            }
            if (!IsStatusSet(StHeader))
            {
                if (source.Remaining() < 2)
                {
                    return false;
                }
                header = source.GetShort();
                SetStatus(StHeader);
            }
            if (!IsStatusSet(StPartition))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                partitionId = source.GetInt();
                SetStatus(StPartition);
            }
            return base.ReadFrom(source);
        }

        /// <summary>Returns an estimation of the packet, including its payload, in bytes.</summary>
        /// <remarks>Returns an estimation of the packet, including its payload, in bytes.</remarks>
        /// <returns>the size of the packet.</returns>
        public int Size()
        {
            // 7 = byte(version) + short(header) + int(partitionId)
            return (data != null ? GetDataSize(data, context) : 0) + 7;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Packet{");
            sb.Append("header=").Append(header);
            sb.Append(", isResponse=").Append(IsHeaderSet(Hazelcast.IO.Packet.HeaderResponse));
            sb.Append(", isOperation=").Append(IsHeaderSet(Hazelcast.IO.Packet.HeaderOp));
            sb.Append(", isEvent=").Append(IsHeaderSet(Hazelcast.IO.Packet.HeaderEvent));
            sb.Append(", partitionId=").Append(partitionId);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
