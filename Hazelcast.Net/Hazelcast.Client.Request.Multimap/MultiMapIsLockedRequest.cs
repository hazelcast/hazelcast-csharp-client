using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class MultiMapIsLockedRequest : AbstractIsLockedRequest, IRetryableRequest
    {
        internal string name;

        public MultiMapIsLockedRequest(Data key, string name) : base(key)
        {
            this.name = name;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            base.WritePortable(writer);
        }

        public override int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.IsLocked;
        }
    }
}