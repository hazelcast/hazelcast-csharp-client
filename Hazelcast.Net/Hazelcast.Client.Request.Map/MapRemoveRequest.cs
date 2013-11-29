using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapRemoveRequest : IPortable
    {
        protected internal Data key;
        protected internal string name;

        protected internal int threadId;

        public MapRemoveRequest()
        {
        }

        public MapRemoveRequest(string name, Data key, int threadId)
        {
            this.name = name;
            this.key = key;
            this.threadId = threadId;
        }

        public virtual int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MapPortableHook.Remove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteInt("t", threadId);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            threadId = reader.ReadInt("t");
            IObjectDataInput input = reader.GetRawDataInput();
            key = new Data();
            key.ReadData(input);
        }

        public virtual object GetKey()
        {
            return key;
        }
    }
}