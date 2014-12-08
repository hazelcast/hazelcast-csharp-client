using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class KeyBasedContainsRequest : MultiMapKeyBasedRequest
    {
        private readonly long threadId;
        private readonly IData value;

        public KeyBasedContainsRequest(string name, IData key, IData value) : base(name, key)
        {
            this.value = value;
        }

        public KeyBasedContainsRequest(string name, IData key, IData value, long threadId) : base(name, key)
        {
            this.value = value;
            this.threadId = threadId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.KeyBasedContains;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("threadId", threadId);
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(value);
        }
    }
}