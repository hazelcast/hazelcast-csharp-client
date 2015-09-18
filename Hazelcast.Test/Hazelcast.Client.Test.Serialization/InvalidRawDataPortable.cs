using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class InvalidRawDataPortable : RawDataPortable
    {
        public InvalidRawDataPortable()
        {
        }

        public InvalidRawDataPortable(long l, char[] c, NamedPortable p, int k, string s, ByteArrayDataSerializable sds)
            :
                base(l, c, p, k, s, sds)
        {
        }

        public override int GetClassId()
        {
            return TestSerializationConstants.INVALID_RAW_DATA_PORTABLE;
        }

        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("l", l);
            var output = writer.GetRawDataOutput();
            output.WriteInt(k);
            output.WriteUTF(s);
            writer.WriteCharArray("c", c);
            output.WriteObject(sds);
            writer.WritePortable("p", p);
        }
    }
}