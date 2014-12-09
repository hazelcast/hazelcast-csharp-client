using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Countdownlatch
{
    internal sealed class GetCountRequest : ClientRequest, IRetryableRequest
    {
        private string name;


        public GetCountRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return CountDownLatchPortableHook.FId;
        }

        public override int GetClassId()
        {
            return CountDownLatchPortableHook.GetCount;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
        }

    }
}