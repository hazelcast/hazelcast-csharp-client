using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.IO;

namespace Hazelcast.Client.Tests.gcpd.HazelcastClientTest
{
    class DataWrapper : DataSerializable
    {
        private byte[] bytes;
        private long fromBytes;
        private long toBytes;

        public byte[] Bytes { 
            get { return bytes; } 
            set { bytes = value; } 
        }

        public long FromBytes 
        {
            get { return fromBytes; } 
            set { fromBytes = value; } 
        }

        public long ToBytes 
        {
            get { return toBytes; }
            set { toBytes = value; } 
        }

        public DataWrapper()
        {
        }

        public DataWrapper(long fromBytes, long toBytes, byte[] bytes)
        {
            this.fromBytes = fromBytes;
            this.toBytes = toBytes;
            this.bytes = bytes;

        }

        public void readData(IDataInput din)
        {
            fromBytes = din.readLong();
            toBytes = din.readLong();
            bytes = new byte[(int)(toBytes - fromBytes) + 1];
            din.readFully(bytes);
        }

        public void writeData(IDataOutput dout)
        {
            dout.writeLong(fromBytes);
            dout.writeLong(toBytes);
            dout.write(bytes);
        }
    }
}
