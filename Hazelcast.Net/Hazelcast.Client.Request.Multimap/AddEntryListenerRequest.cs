using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class AddEntryListenerRequest : ClientRequest
    {
        internal bool includeValue;
        internal IData key;
        internal string name;

        public AddEntryListenerRequest(string name, IData key, bool includeValue)
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
        public override void Write(IPortableWriter writer)
        {
            writer.WriteBoolean("i", includeValue);
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}