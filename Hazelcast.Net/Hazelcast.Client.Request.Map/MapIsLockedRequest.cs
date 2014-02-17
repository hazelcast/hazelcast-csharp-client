using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapIsLockedRequest : AbstractIsLockedRequest, IRetryableRequest
    {
        private string name;

        public MapIsLockedRequest()
        {
        }

        public MapIsLockedRequest(string name, Data key) : base(key)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.IsLocked;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            base.WritePortable(writer);
        }

    }
}