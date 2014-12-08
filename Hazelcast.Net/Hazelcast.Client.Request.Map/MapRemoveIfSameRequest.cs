using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapRemoveIfSameRequest : ClientRequest
    {
        protected internal IData key;
        protected internal string name;

        protected internal long threadId;
        protected internal IData value;


        public MapRemoveIfSameRequest(string name, IData key, IData value, long threadId)
        {
            this.name = name;
            this.key = key;
            this.value = value;
            this.threadId = threadId;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.RemoveIfSame;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", threadId);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
            output.WriteData(value);
        }
    }
}