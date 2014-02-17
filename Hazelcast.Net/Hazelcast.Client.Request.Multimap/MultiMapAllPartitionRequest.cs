using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal abstract class MultiMapAllPartitionRequest : ClientRequest
    {
        internal string name;

        protected internal MultiMapAllPartitionRequest()
        {
        }

        protected internal MultiMapAllPartitionRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
        }

    }
}