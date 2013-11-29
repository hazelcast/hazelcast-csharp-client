using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class AddListenerRequest : IPortable
    {
        private bool includeValue;
        private string name;

        public AddListenerRequest()
        {
        }

        public AddListenerRequest(string name, bool includeValue)
        {
            this.name = name;
            this.includeValue = includeValue;
        }

        public virtual int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return QueuePortableHook.AddListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteBoolean("i", includeValue);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            includeValue = reader.ReadBoolean("i");
        }
    }
}