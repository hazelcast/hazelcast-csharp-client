using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Countdownlatch
{
    internal sealed class SetCountRequest : ClientRequest
    {
        private int count;
        private string name;


        public SetCountRequest(string name, int count)
        {
            this.name = name;
            this.count = count;
        }

        public override int GetFactoryId()
        {
            return CountDownLatchPortableHook.FId;
        }

        public override int GetClassId()
        {
            return CountDownLatchPortableHook.SetCount;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteInt("count", count);
        }


    }
}