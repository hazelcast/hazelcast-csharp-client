using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{

    public class ComplexDataSerializable : IDataSerializable
    {
        private IDataSerializable ds;
        private IDataSerializable ds2;
        private IPortable portable;

        public ComplexDataSerializable()
        {
        }

        public ComplexDataSerializable(IPortable portable, IDataSerializable ds, IDataSerializable ds2)
        {
            this.portable = portable;
            this.ds = ds;
            this.ds2 = ds2;
        }

        public void ReadData(IObjectDataInput input)
        {
            ds = input.ReadObject<IDataSerializable>();
            portable = input.ReadObject<IPortable>();
            ds2 = input.ReadObject<IDataSerializable>();
        }

        public string GetJavaClassName()
        {
            return typeof (ComplexDataSerializable).Name;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(ds);
            output.WriteObject(portable);
            output.WriteObject(ds2);
        }

        protected bool Equals(ComplexDataSerializable other)
        {
            return Equals(ds, other.ds) && Equals(ds2, other.ds2) && Equals(portable, other.portable);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ComplexDataSerializable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ds != null ? ds.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ds2 != null ? ds2.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (portable != null ? portable.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}