using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapTryPutRequest : MapPutRequest
    {
        private readonly long timeout;


        public MapTryPutRequest(string name, IData key, IData value, long threadId, long timeout)
            : base(name, key, value, threadId, -1)
        {
            this.timeout = timeout;
        }

        public override int GetClassId()
        {
            return MapPortableHook.TryPut;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("timeout", timeout);
            base.Write(writer);
        }
    }
}