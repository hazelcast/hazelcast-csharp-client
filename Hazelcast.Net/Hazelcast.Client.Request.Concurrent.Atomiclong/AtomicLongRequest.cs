using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    public abstract class AtomicLongRequest : IPortable
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

        public virtual int GetFactoryId()
        {
            return AtomicLongPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("d", delta);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            delta = reader.ReadLong("d");
        }

        public abstract int GetClassId();
    }
}