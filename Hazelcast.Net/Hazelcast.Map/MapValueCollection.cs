using System;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Map
{
    internal class MapValueCollection : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        internal ICollection<IData> values;

        public MapValueCollection(ICollection<IData> values)
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
            foreach (IData o in values)
            {
                output.WriteData(o);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput input)
        {
            int size = input.ReadInt();
            values = new List<IData>(size);
            for (int i = 0; i < size; i++)
            {
                IData data = input.ReadData();
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

        public virtual ICollection<IData> GetValues()
        {
            return values;
        }
    }
}