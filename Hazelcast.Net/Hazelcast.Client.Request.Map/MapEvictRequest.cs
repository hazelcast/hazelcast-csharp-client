using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapEvictRequest : ClientRequest
    {
        private Data key;
        private string name;

        private long threadId;

        public MapEvictRequest(string name, Data key, long threadId)
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
            return MapPortableHook.Evict;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", threadId);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }

    }
}