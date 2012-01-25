using System;
using Hazelcast.IO;
using Hazelcast.Client.IO;

namespace Hazelcast.Client
{
	public class SerializationHelper
	{
		public SerializationHelper ()
		{
		}
		public static void writeObject(IDataOutput dout, Object obj){
        if (obj == null) {
            dout.writeByte(0);
        } else if (obj is long) {
            dout.writeByte(1);
            dout.writeLong((long) obj);
        } else if (obj is int) {
            dout.writeByte(2);
            dout.writeInt((int) obj);
        } else if (obj is String) {
            dout.writeByte(3);
            dout.writeUTF((String) obj);
        } else if (obj is double) {
            dout.writeByte(4);
            dout.writeDouble((double) obj);
        } else if (obj is float) {
            dout.writeByte(5);
            dout.writeFloat((float) obj);
        } else if (obj is bool) {
            dout.writeByte(6);
            dout.writeBoolean((bool) obj);
        } else if (obj is DataSerializable) {
            dout.writeByte(7);
			DataSerializable ds = (DataSerializable)obj;
            dout.writeUTF(ds.javaClassName());
            ds.writeData(dout);
        } else if (obj is System.DateTime) {
            dout.writeByte(8);
            //dout.writeLong(((DateTime) obj).Tot);
        } else {
            byte[] buf = Hazelcast.Client.IO.IOUtil.toByte(obj);
            dout.writeInt(buf.Length);
            dout.write(buf);
        }
    }

    public static Object readObject(IDataInput din) {
        byte type = din.readByte();
        if (type == 0) {
            return null;
        } else if (type == 1) {
            return din.readLong();
        } else if (type == 2) {
            return din.readInt();
        } else if (type == 3) {
            return din.readUTF();
        } else if (type == 4) {
            return din.readDouble();
        } else if (type == 5) {
            return din.readFloat();
        } else if (type == 6) {
            return din.readBoolean();
        } else if (type == 7) {
            DataSerializable ds;
            String className = din.readUTF();
            ds = (DataSerializable) Hazelcast.Client.IO.DataSerializer.createInstance(className);
            ds.readData(din);
            return ds;
        } else if (type == 8) {
			long d = din.readLong();
			System.DateTime dt = new System.DateTime(1970, 1, 1);
			dt.AddMilliseconds(d);
            return dt;
        } else if (type == 9) {
            int len = din.readInt();
            byte[] buf = new byte[len];
            din.readFully(buf);
			return IOUtil.toObject(buf);	
        } else 
            throw new Exception("Unknown object type=" + type);
    }

    public static void writeByteArray(IDataOutput dout, byte[] value) {
        int size = (value == null) ? 0 : value.Length;
        dout.writeInt(size);
        if (size > 0) {
            dout.write(value);
        }
    }

    public static byte[] readByteArray(IDataInput din) {
        int size = din.readInt();
        if (size == 0) {
            return null;
        } else {
            byte[] b = new byte[size];
            din.readFully(b);
            return b;
        }
    }
	}
}

