using System;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Map
{
    internal sealed class MapEntrySet : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        private readonly ICollection<KeyValuePair<IData, IData>> entrySet;

        public MapEntrySet()
        {
            entrySet = new HashSet<KeyValuePair<IData, IData>>();
        }

        public MapEntrySet(ICollection<KeyValuePair<IData, IData>> entrySet)
        {
            this.entrySet = entrySet;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            int size = entrySet.Count;
            output.WriteInt(size);
            foreach (KeyValuePair<IData, IData> o in entrySet)
            {
                output.WriteData(o.Key);
                output.WriteData(o.Value);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            int size = input.ReadInt();
            for (int i = 0; i < size; i++)
            {
                IData key = input.ReadData();
                IData value = input.ReadData();
                var entry = new KeyValuePair<IData, IData>(key, value);
                entrySet.Add(entry);
            }
        }

        public int GetFactoryId()
        {
            return MapDataSerializerHook.FId;
        }

        public int GetId()
        {
            return MapDataSerializerHook.EntrySet;
        }

        public ICollection<KeyValuePair<IData, IData>> GetEntrySet()
        {
            return entrySet;
        }

        public void Add(KeyValuePair<IData, IData> entry)
        {
            entrySet.Add(entry);
        }

        public void Add(Data key, Data value)
        {
            entrySet.Add(new KeyValuePair<IData, IData>(key, value));
        }
    }
}