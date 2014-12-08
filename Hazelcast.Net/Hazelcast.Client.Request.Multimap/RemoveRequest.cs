using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class RemoveRequest : MultiMapKeyBasedRequest
    {
        internal long threadId;
        internal IData value;

        public RemoveRequest(string name, IData key, IData value, long threadId)
            : base(name, key)
        {
            this.value = value;
            this.threadId = threadId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Remove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("t", threadId);
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(value);
        }
    }
}