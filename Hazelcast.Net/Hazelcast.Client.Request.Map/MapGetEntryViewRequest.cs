using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapGetEntryViewRequest : ClientRequest, IRetryableRequest
    {
        private Data key;
        private string name;

        public MapGetEntryViewRequest()
        {
        }

        public MapGetEntryViewRequest(string name, Data key)
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
            return MapPortableHook.GetEntryView;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }

        public virtual object GetKey()
        {
            return key;
        }
    }
}