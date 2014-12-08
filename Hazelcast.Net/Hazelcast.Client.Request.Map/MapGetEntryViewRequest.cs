using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapGetEntryViewRequest : ClientRequest, IRetryableRequest
    {
        private readonly IData key;
        private readonly string name;
        private readonly long threadId;

        public MapGetEntryViewRequest()
        {
        }

        public MapGetEntryViewRequest(string name, IData key, long threadId)
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
            return MapPortableHook.GetEntryView;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("threadId", threadId);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}