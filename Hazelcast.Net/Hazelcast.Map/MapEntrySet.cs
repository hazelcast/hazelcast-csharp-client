using System;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Map
{
    [Serializable]
    public sealed class MapEntrySet : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        private readonly ICollection<KeyValuePair<Data, Data>> entrySet;

        public MapEntrySet()
        {
            entrySet = new HashSet<KeyValuePair<Data, Data>>();
        }

        public MapEntrySet(ICollection<KeyValuePair<Data, Data>> entrySet)
        {
            this.entrySet = entrySet;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            int size = entrySet.Count;
            output.WriteInt(size);
            foreach (var o in entrySet)
            {
                o.Key.WriteData(output);
                o.Value.WriteData(output);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            int size = input.ReadInt();
            for (int i = 0; i < size; i++)
            {
                var entry = new KeyValuePair<Data, Data>(IOUtil.ReadData(input), IOUtil.ReadData(input));
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

        public ICollection<KeyValuePair<Data, Data>> GetEntrySet()
        {
            return entrySet;
        }

        public void Add(KeyValuePair<Data, Data> entry)
        {
            entrySet.Add(entry);
        }

        public void Add(Data key, Data value)
        {
            entrySet.Add(new KeyValuePair<Data, Data>(key, value));
        }
    }
}