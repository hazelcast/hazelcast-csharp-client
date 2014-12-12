using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class PutRequest : MultiMapKeyBasedRequest
    {
        internal int index = -1;
        internal long threadId;
        internal IData value;

        public PutRequest(string name, IData key, IData value, int index, long threadId) : base(name, key)
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
        public override void Write(IPortableWriter writer)
        {
            writer.WriteInt("i", index);
            writer.WriteLong("t", threadId);
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(value);
        }
    }
}