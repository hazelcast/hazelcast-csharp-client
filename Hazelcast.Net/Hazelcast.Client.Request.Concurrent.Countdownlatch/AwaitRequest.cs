using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Countdownlatch
{
    public sealed class AwaitRequest : IPortable
    {
        private string name;

        private long timeout;

        public AwaitRequest()
        {
        }

        public AwaitRequest(string name, long timeout)
        {
            this.name = name;
            this.timeout = timeout;
        }

        public int GetFactoryId()
        {
            return CountDownLatchPortableHook.FId;
        }

        public int GetClassId()
        {
            return CountDownLatchPortableHook.Await;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteLong("timeout", timeout);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("name");
            timeout = reader.ReadLong("timeout");
        }
    }
}