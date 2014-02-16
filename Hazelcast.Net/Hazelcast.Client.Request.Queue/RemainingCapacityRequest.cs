using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class RemainingCapacityRequest : ClientRequest, IRetryableRequest
    {
        protected internal string name;

        public RemainingCapacityRequest()
        {
        }

        public RemainingCapacityRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.RemainingCapacity;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
        }


    }
}