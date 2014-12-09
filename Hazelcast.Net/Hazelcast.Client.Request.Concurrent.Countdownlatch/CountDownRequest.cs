using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Countdownlatch
{
    internal sealed class CountDownRequest : ClientRequest
    {
        private string name;


        public CountDownRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return CountDownLatchPortableHook.FId;
        }

        public override int GetClassId()
        {
            return CountDownLatchPortableHook.CountDown;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
        }

    }
}