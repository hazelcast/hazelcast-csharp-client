using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class RemoveRequest : MultiMapKeyBasedRequest
    {
        internal int threadId;
        internal Data value;

        public RemoveRequest()
        {
        }

        public RemoveRequest(string name, Data key, Data value, int threadId) : base(name, key)
        {
            this.value = value;
            this.threadId = threadId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Remove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("t", threadId);
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            value.WriteData(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            threadId = reader.ReadInt("t");
            base.ReadPortable(reader);
            IObjectDataInput input = reader.GetRawDataInput();
            value = new Data();
            value.ReadData(input);
        }
    }
}