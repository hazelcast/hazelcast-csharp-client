using System;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Map
{
    internal class MapKeySet : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        internal ICollection<IData> keySet;

        public MapKeySet(ICollection<IData> keySet)
        {
            this.keySet = keySet;
        }

        public MapKeySet()
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IObjectDataOutput output)
        {
            int size = keySet.Count;
            output.WriteInt(size);
            foreach (IData o in keySet)
            {
                output.WriteData(o);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput input)
        {
            int size = input.ReadInt();
            keySet = new HashSet<IData>();
            for (int i = 0; i < size; i++)
            {
                IData data = input.ReadData();
                keySet.Add(data);
            }
        }
        public virtual int GetFactoryId()
        {
            return MapDataSerializerHook.FId;
        }

        public virtual int GetId()
        {
            return MapDataSerializerHook.KeySet;
        }

        public virtual ICollection<IData> GetKeySet()
        {
            return keySet;
        }
    }
}