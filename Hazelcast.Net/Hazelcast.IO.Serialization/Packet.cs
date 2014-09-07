using System;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class Packet : DataAdapter
    {
        public static readonly byte Version = 2;
        public static readonly int HeaderOp = 0;
        public static readonly int HeaderResponse = 1;
        public static readonly int HeaderEvent = 2;
        public static readonly int HeaderWanReplication = 3;
        public static readonly int HeaderUrgent = 4;

        private const int StVersion = 11;
        private const int StHeader = 12;
        private const int StPartition = 13;

        private short header;
        private int partitionId;


        public Packet(IPortableContext context)
            : base(context)
        {
        }

        public Packet(Data value, IPortableContext context)
            : this(value, -1, context)
        {
        }

        public Packet(Data value, int partitionId, IPortableContext context)
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

        public short getHeader()
        {
            return header;
        }

        public int getPartitionId()
        {
            return partitionId;
        }

        public bool isUrgent()
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
                BitConverter.GetBytes(Version);
                destination.Put(Version);
                SetStatus(StVersion);
            }
            if (!IsStatusSet(StHeader))
            {
                if (destination.Remaining() < 2)
                {
                    return false;
                }
                destination.PutShort(header);
                SetStatus(StHeader);
            }
            if (!IsStatusSet(StPartition))
            {
                if (destination.Remaining() < 4)
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
                    throw new ArgumentException("Packet versions are not matching! This -> "
                                                + Version + ", Incoming -> " + version);
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
    }
}