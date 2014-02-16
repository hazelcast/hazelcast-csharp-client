using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapContainsKeyRequest : ClientRequest, IRetryableRequest
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

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.ContainsKey;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }


    }
}