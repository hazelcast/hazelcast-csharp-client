using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapTryPutRequest : MapPutRequest
    {
        private long timeout;

        public MapTryPutRequest()
        {
        }

        public MapTryPutRequest(string name, Data key, Data value, int threadId, long timeout)
            : base(name, key, value, threadId, -1)
        {
            this.timeout = timeout;
        }

        public override int GetClassId()
        {
            return MapPortableHook.TryPut;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("timeout", timeout);
            base.WritePortable(writer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            timeout = reader.ReadLong("timeout");
            base.ReadPortable(reader);
        }
    }
}