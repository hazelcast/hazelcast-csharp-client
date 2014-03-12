using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapPutAllRequest : ClientRequest
    {
        private MapEntrySet entrySet;
        protected internal string name;

        public MapPutAllRequest(string name, MapEntrySet entrySet)
        {
            this.name = name;
            this.entrySet = entrySet;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.PutAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            entrySet.WriteData(output);
        }

    }
}