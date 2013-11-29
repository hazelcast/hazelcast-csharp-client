using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class RemainingCapacityRequest : IPortable, IRetryableRequest
    {
        protected internal string name;

        public RemainingCapacityRequest()
        {
        }

        public RemainingCapacityRequest(string name)
        {
            this.name = name;
        }

        public virtual int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return QueuePortableHook.RemainingCapacity;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
        }
    }
}