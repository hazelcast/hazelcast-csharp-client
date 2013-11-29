using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class PutRequest : MultiMapKeyBasedRequest
    {
        internal int index = -1;

        internal int threadId = -1;
        internal Data value;

        public PutRequest()
        {
        }

        public PutRequest(string name, Data key, Data value, int index, int threadId) : base(name, key)
        {
            this.value = value;
            this.index = index;
            this.threadId = threadId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Put;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("i", index);
            writer.WriteInt("t", threadId);
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            value.WriteData(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            index = reader.ReadInt("i");
            threadId = reader.ReadInt("t");
            base.ReadPortable(reader);
            IObjectDataInput input = reader.GetRawDataInput();
            value = new Data();
            value.ReadData(input);
        }
    }
}