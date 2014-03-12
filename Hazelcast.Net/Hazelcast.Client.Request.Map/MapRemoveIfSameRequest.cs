using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapRemoveIfSameRequest : ClientRequest
    {
        protected internal Data key;
        protected internal string name;

        protected internal long threadId;
        protected internal Data value;


        public MapRemoveIfSameRequest(string name, Data key, Data value, long threadId)
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
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", threadId);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
            value.WriteData(output);
        }
    }
}