using System;
using System.Linq;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    public class ByteArrayDataSerializable : IDataSerializable
    {
        private byte[] data;

        public ByteArrayDataSerializable()
        {
        }

        public ByteArrayDataSerializable(byte[] data)
        {
            this.data = data;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(data.Length);
            output.Write(data);
        }

        public void ReadData(IObjectDataInput input)
        {
            var len = input.ReadInt();
            data = new byte[len];
            input.ReadFully(data);
        }

        public string GetJavaClassName()
        {
            return typeof (ByteArrayDataSerializable).Name;
        }

        protected bool Equals(ByteArrayDataSerializable other)
        {
            return data.SequenceEqual(other.data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ByteArrayDataSerializable) obj);
        }

        public override int GetHashCode()
        {
            return (data != null ? data.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Data: {0}", string.Join(", ", data));
        }
    }
}
