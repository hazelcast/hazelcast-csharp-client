using System.Collections;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Spi
{
    public sealed class PortableCollection : IPortable
    {
        private ICollection<Data> collection;

        public PortableCollection()
        {
        }

        public PortableCollection(ICollection<Data> collection)
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
            foreach (Data data in collection)
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
                collection = new List<Data>(size);
            }
            else
            {
                collection = new HashSet<Data>();
            }
            IObjectDataInput input = reader.GetRawDataInput();
            for (int i = 0; i < size; i++)
            {
                var data = new Data();
                data.ReadData(input);
                collection.Add(data);
            }
        }

        public ICollection<Data> GetCollection()
        {
            return collection;
        }
    }
}