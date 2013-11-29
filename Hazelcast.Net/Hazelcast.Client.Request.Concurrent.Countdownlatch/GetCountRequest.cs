using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Countdownlatch
{
    public sealed class GetCountRequest : IPortable, IRetryableRequest
    {
        private string name;

        public GetCountRequest()
        {
        }

        public GetCountRequest(string name)
        {
            this.name = name;
        }

        public int GetFactoryId()
        {
            return CountDownLatchPortableHook.FId;
        }

        public int GetClassId()
        {
            return CountDownLatchPortableHook.GetCount;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("name");
        }
    }
}