using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal abstract class MultiMapRequest : ClientRequest
    {
        internal string name;

        protected MultiMapRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
        }
    }
}