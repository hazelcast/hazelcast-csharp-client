using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
    public abstract class SemaphoreRequest : IPortable
    {
        internal string name;

        internal int permitCount;

        protected internal SemaphoreRequest()
        {
        }

        protected internal SemaphoreRequest(string name, int permitCount)
        {
            this.name = name;
            this.permitCount = permitCount;
        }

        public virtual int GetFactoryId()
        {
            return SemaphorePortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteInt("p", permitCount);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            permitCount = reader.ReadInt("p");
        }

        public abstract int GetClassId();
    }
}