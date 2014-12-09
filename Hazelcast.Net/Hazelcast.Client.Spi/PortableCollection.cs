using System.Collections;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Spi
{
    internal sealed class PortableCollection : IPortable
    {
        private ICollection<IData> collection;

        public PortableCollection()
        {
        }

        public PortableCollection(ICollection<IData> collection)
        {
            this.collection = collection;
        }

        public int GetFactoryId()
        {
            return SpiPortableHook.Id;
        }

        public int GetClassId()
        {
            return SpiPortableHook.Collection;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteBoolean("l", collection is IList);
            if (collection == null)
            {
                writer.WriteInt("s", -1);
                return;
            }
            writer.WriteInt("s", collection.Count);
            IObjectDataOutput output = writer.GetRawDataOutput();
            foreach (IData data in collection)
            {
                data.WriteData(output);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadPortable(IPortableReader reader)
        {
            bool list = reader.ReadBoolean("l");
            int size = reader.ReadInt("s");
            if (size == -1)
            {
                return;
            }
            if (list)
            {
                collection = new List<IData>(size);
            }
            else
            {
                collection = new HashSet<IData>();
            }
            IObjectDataInput input = reader.GetRawDataInput();
            for (int i = 0; i < size; i++)
            {
                var data = new IData();
                data.ReadData(input);
                collection.Add(data);
            }
        }

        public ICollection<IData> GetCollection()
        {
            return collection;
        }
    }
}