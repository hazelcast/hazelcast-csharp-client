using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class AddEntryListenerRequest : IPortable
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

        public virtual int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MultiMapPortableHook.AddEntryListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteBoolean("i", includeValue);
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            IOUtil.WriteNullableData(output, key);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            includeValue = reader.ReadBoolean("i");
            name = reader.ReadUTF("n");
            IObjectDataInput input = reader.GetRawDataInput();
            key = IOUtil.ReadNullableData(input);
        }
    }
}