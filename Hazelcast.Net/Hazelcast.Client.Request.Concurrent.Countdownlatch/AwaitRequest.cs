using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Countdownlatch
{
    internal sealed class AwaitRequest : ClientRequest
    {
        private string name;

        private long timeout;

        public AwaitRequest(string name, long timeout)
        {
            this.name = name;
            this.timeout = timeout;
        }

        public override int GetFactoryId()
        {
            return CountDownLatchPortableHook.FId;
        }

        public override int GetClassId()
        {
            return CountDownLatchPortableHook.Await;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteLong("timeout", timeout);
        }

    }
}