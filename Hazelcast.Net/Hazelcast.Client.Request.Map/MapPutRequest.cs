using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapPutRequest : IPortable
    {
        protected internal Data key;

        protected internal string name;

        protected internal int threadId;

        protected internal long ttl;
        protected internal Data value;

        public MapPutRequest()
        {
        }

        public MapPutRequest(string name, Data key, Data value, int threadId, long ttl)
        {
            this.name = name;
            this.key = key;
            this.value = value;
            this.threadId = threadId;
            this.ttl = ttl;
        }

        public MapPutRequest(string name, Data key, Data value, int threadId)
        {
            this.name = name;
            this.key = key;
            this.value = value;
            this.threadId = threadId;
            ttl = -1;
        }

        public virtual int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MapPortableHook.Put;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteInt("t", threadId);
            writer.WriteLong("ttl", ttl);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
            value.WriteData(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            threadId = reader.ReadInt("t");
            ttl = reader.ReadLong("ttl");
            IObjectDataInput input = reader.GetRawDataInput();
            key = new Data();
            key.ReadData(input);
            value = new Data();
            value.ReadData(input);
        }
    }
}