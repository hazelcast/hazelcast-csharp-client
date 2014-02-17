using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class AddEntryListenerRequest : ClientRequest
    {
        internal bool includeValue;
        internal Data key;
        internal string name;

        public AddEntryListenerRequest()
        {
        }

        public AddEntryListenerRequest(string name, Data key, bool includeValue)
        {
            this.name = name;
            this.key = key;
            this.includeValue = includeValue;
        }

        public override int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.AddEntryListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteBoolean("i", includeValue);
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            IOUtil.WriteNullableData(output, key);
        }

    }
}