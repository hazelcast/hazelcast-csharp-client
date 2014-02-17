using System;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Map
{
    [Serializable]
    internal class MapValueCollection : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        internal ICollection<Data> values;

        public MapValueCollection(ICollection<Data> values)
        {
            this.values = values;
        }

        public MapValueCollection()
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IObjectDataOutput output)
        {
            int size = values.Count;
            output.WriteInt(size);
            foreach (Data o in values)
            {
                o.WriteData(output);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput input)
        {
            int size = input.ReadInt();
            values = new List<Data>(size);
            for (int i = 0; i < size; i++)
            {
                var data = new Data();
                data.ReadData(input);
                values.Add(data);
            }
        }


        public virtual int GetFactoryId()
        {
            return MapDataSerializerHook.FId;
        }

        public virtual int GetId()
        {
            return MapDataSerializerHook.Values;
        }

        public virtual ICollection<Data> GetValues()
        {
            return values;
        }
    }
}