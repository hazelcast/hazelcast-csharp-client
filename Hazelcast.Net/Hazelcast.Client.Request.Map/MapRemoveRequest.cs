using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapRemoveRequest : ClientRequest
    {
        private bool async;
        protected internal IData key;
        protected internal string name;

        protected internal long threadId;

        public MapRemoveRequest(string name, IData key, long threadId)
        {
            this.name = name;
            this.key = key;
            this.threadId = threadId;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.Remove;
        }

        public void SetAsync(bool async)
        {
            this.async = async;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", threadId);
            writer.WriteBoolean("a", async);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}