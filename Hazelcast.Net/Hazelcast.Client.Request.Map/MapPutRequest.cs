using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapPutRequest : ClientRequest
    {
        private bool async;
        protected internal IData key;

        protected internal string name;

        protected internal long threadId;

        protected internal long ttl;
        protected internal IData value;

        public MapPutRequest(string name, IData key, IData value, long threadId, long ttl)
        {
            this.name = name;
            this.key = key;
            this.value = value;
            this.threadId = threadId;
            this.ttl = ttl;
        }

        public MapPutRequest(string name, IData key, IData value, long threadId)
        {
            this.name = name;
            this.key = key;
            this.value = value;
            this.threadId = threadId;
            ttl = -1;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.Put;
        }

        public void SetAsAsync()
        {
            this.async = true;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", threadId);
            writer.WriteLong("ttl", ttl);
            writer.WriteBoolean("a", async);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
            output.WriteData(value);
        }
    }
}