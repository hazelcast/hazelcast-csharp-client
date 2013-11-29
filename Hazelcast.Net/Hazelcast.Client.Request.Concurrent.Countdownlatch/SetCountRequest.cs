using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Countdownlatch
{
    public sealed class SetCountRequest : IPortable
    {
        private int count;
        private string name;

        public SetCountRequest()
        {
        }

        public SetCountRequest(string name, int count)
        {
            this.name = name;
            this.count = count;
        }

        public int GetFactoryId()
        {
            return CountDownLatchPortableHook.FId;
        }

        public int GetClassId()
        {
            return CountDownLatchPortableHook.SetCount;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteInt("count", count);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("name");
            count = reader.ReadInt("count");
        }
    }
}