using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapPutRequest : ClientRequest
    {
        protected internal Data key;

        protected internal string name;

        protected internal long threadId;

        protected internal long ttl;
        protected internal Data value;

        public MapPutRequest()
        {
        }

        public MapPutRequest(string name, Data key, Data value, long threadId, long ttl)
        {
            this.name = name;
            this.key = key;
            this.value = value;
            this.threadId = threadId;
            this.ttl = ttl;
        }

        public MapPutRequest(string name, Data key, Data value, long threadId)
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

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", threadId);
            writer.WriteLong("ttl", ttl);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
            value.WriteData(output);
        }

    }
}