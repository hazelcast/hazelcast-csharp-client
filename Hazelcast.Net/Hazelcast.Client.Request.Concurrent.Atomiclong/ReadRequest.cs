using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    internal abstract class ReadRequest : ClientRequest
    {
        protected internal string name;

        protected ReadRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return AtomicLongPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
        }
    }
}