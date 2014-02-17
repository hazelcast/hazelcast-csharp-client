using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class RemoveRequest : MultiMapKeyBasedRequest
    {
        internal long threadId;
        internal Data value;

        public RemoveRequest()
        {
        }

        public RemoveRequest(string name, Data key, Data value, long threadId) : base(name, key)
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
            writer.WriteLong("t", threadId);
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            value.WriteData(output);
        }

    }
}