using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
    internal abstract class SemaphoreRequest : ClientRequest
    {
        internal string name;

        internal int permitCount;


        protected internal SemaphoreRequest(string name, int permitCount)
        {
            this.name = name;
            this.permitCount = permitCount;
        }

        public override int GetFactoryId()
        {
            return SemaphorePortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteInt("p", permitCount);
        }

    }
}