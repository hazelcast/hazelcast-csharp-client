using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapContainsKeyRequest : IPortable, IRetryableRequest
    {
        private Data key;
        private string name;

        public MapContainsKeyRequest()
        {
        }

        public MapContainsKeyRequest(string name, Data key)
        {
            this.name = name;
            this.key = key;
        }

        public virtual int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MapPortableHook.ContainsKey;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            IObjectDataInput input = reader.GetRawDataInput();
            key = new Data();
            key.ReadData(input);
        }
    }
}