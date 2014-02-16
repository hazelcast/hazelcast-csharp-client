using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    public abstract class AtomicLongRequest : ClientRequest
    {
        internal long delta;
        internal string name;

        protected internal AtomicLongRequest()
        {
        }

        protected internal AtomicLongRequest(string name, long delta)
        {
            this.name = name;
            this.delta = delta;
        }

        public override int GetFactoryId()
        {
            return AtomicLongPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("d", delta);
        }

    }
}