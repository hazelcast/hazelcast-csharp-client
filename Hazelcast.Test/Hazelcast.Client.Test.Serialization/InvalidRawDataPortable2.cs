using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class InvalidRawDataPortable2 : RawDataPortable
    {
        public InvalidRawDataPortable2()
        {
        }

        public InvalidRawDataPortable2(long l, char[] c, NamedPortable p, int k, string s, ByteArrayDataSerializable sds)
            :
                base(l, c, p, k, s, sds)
        {
        }

        public override int GetClassId()
        {
            return TestSerializationConstants.INVALID_RAW_DATA_PORTABLE_2;
        }

        public override void ReadPortable(IPortableReader reader)
        {
            c = reader.ReadCharArray("c");
            var input = reader.GetRawDataInput();
            k = input.ReadInt();
            l = reader.ReadLong("l");
            s = input.ReadUTF();
            p = reader.ReadPortable<NamedPortable>("p");
            sds = input.ReadObject<ByteArrayDataSerializable>();
        }
    }
}