using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class ComplexDataSerializable : IDataSerializable
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
    }
}