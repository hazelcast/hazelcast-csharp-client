using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class MultiMapUnlockRequest : AbstractUnlockRequest
    {
        internal string name;

        public MultiMapUnlockRequest(IData key, int threadId, string name) : base(key, threadId)
        {
            this.name = name;
        }

        public MultiMapUnlockRequest(IData key, int threadId, bool force, string name) : base(key, threadId, force)
        {
            this.name = name;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            base.Write(writer);
        }

        public override int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.Unlock;
        }
    }
}