using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class DataDataSerializable : IDataSerializable
    {
        internal IData Data;

        public DataDataSerializable()
        {
        }

        public DataDataSerializable(IData data)
        {
            this.Data = data;
        }

        public void ReadData(IObjectDataInput input)
        {
            Data = input.ReadData();
        }

        public string GetJavaClassName()
        {
            return typeof (DataDataSerializable).FullName;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteData(Data);
        }

        protected bool Equals(DataDataSerializable other)
        {
            return Equals(Data, other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataDataSerializable) obj);
        }

        public override int GetHashCode()
        {
            return (Data != null ? Data.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Data: {0}", Data);
        }
    }
}