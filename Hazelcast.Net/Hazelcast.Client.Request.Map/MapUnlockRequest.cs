using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapUnlockRequest : AbstractUnlockRequest
    {
        private string name;

        public MapUnlockRequest()
        {
        }

        public MapUnlockRequest(string name, Data key, int threadId) : base(key, threadId, false)
        {
            this.name = name;
        }

        public MapUnlockRequest(string name, Data key, int threadId, bool force) : base(key, threadId, force)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.Unlock;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            base.WritePortable(writer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            base.ReadPortable(reader);
        }
    }
}