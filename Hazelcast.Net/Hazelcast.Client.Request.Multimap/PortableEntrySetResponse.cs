using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class PortableEntrySetResponse : IPortable
    {
        internal ICollection<KeyValuePair<IData, IData>> entrySet = null;

        public PortableEntrySetResponse(ICollection<KeyValuePair<IData, IData>> entrySet)
        {
            this.entrySet = entrySet;
        }

        public virtual int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MultiMapPortableHook.EntrySetResponse;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("s", entrySet.Count);
            IObjectDataOutput output = writer.GetRawDataOutput();
            foreach (var entry in entrySet)
            {
                IData key = entry.Key;
                IData value = entry.Value;
                output.WriteData(key);
                output.WriteData(value);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            int size = reader.ReadInt("s");
            IObjectDataInput input = reader.GetRawDataInput();
            entrySet = new HashSet<KeyValuePair<IData, IData>>();
            for (int i = 0; i < size; i++)
            {
                IData key = input.ReadData();
                IData value = input.ReadData();
                entrySet.Add(new KeyValuePair<IData, IData>(key, value));
            }
        }
    }
}