using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class AddListenerRequest : ClientRequest
    {
        private readonly bool includeValue;
        private readonly string name;

        public AddListenerRequest(string name, bool includeValue)
        {
            this.name = name;
            this.includeValue = includeValue;
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.AddListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteBoolean("i", includeValue);
        }
    }
}